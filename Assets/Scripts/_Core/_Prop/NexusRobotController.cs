using System;
using UnityEngine;
using static ActuatorUtility;

public class NexusRobotController : MonoBehaviour
{
    private GameObject _tracker;

    [Header("シリアル通信")]
    [SerializeField] private bool _useSerial = false;
    [SerializeField] private string _portNum = "COM4";
    [SerializeField] private int _baudRate = 115200;
    private int _serialIndex;

    [Header("距離PIDパラメータ")]
    [SerializeField] private float _pOfDirection = 200;
    [SerializeField] private float _iOfDirection = 0;
    [SerializeField] private float _dOfDirection = 1;

    [Header("角度PIDパラメータ")]
    [SerializeField] private float _pOfAngle = 15;
    [SerializeField] private float _iOfAngle = 0;
    [SerializeField] private float _dOfAngle = 0.1f;

    [Header("最高速度")]
    [SerializeField] private float _maxSpeed = 200f;        //単位は[mm/s] 
    [SerializeField] private float _maxTurnSpeed = 5;    //単位は[deg/s]

    [Header("エラー許容量")]
    [SerializeField] private float _distanceErrorThreshold = 0.01f;     //単位は[m]
    [SerializeField] private float _angleErrorThreshold = 1;        //単位は[deg]

    private float _lastDistaneError = 0;
    private float _lastAngleError = 0;
    private float _distanceErrorSum = 0;
    private float _angleErrorSum = 0;
    private float _lastCalcTime = 0;

    //RVO関連
    private int _rvoIndex;
    readonly float _RVOMovableRange = 0.5f;
    readonly float _RangePropGoToTargetDirectly = 0.8f;

    private Func<Vector3> PropPos;
    private Func<Vector3> PropForward;
    private Func<Vector3> PropRight;
    private Action<RobotMoveParameter> MoveAction;
    private Action StopAction;

    private bool _isPlaced = false;
    public bool IsPlaced
    {
        get
        {
            return _isPlaced;
        }
    }

    private void Start()
    {
        if (_useSerial)
        {
            _tracker = gameObject.GetComponent<Prop>().liftTopTracker;
            _serialIndex = SerialUtility.Instance.Open(_portNum, _baudRate);
            SerialUtility.Instance.Write(_serialIndex, "H");　//これを送らないとメカナムロボと通信できない（Arduinoコード参照）

            PropPos = () => _tracker.transform.position;
            PropForward = () => _tracker.transform.up;
            PropRight = () =>- _tracker.transform.right;
            MoveAction = (para) => SerialUtility.Instance.Write(_serialIndex, FormatSetCarMove(para) + "\n");
            StopAction = () => SerialUtility.Instance.Write(_serialIndex, FormatSetCarMove(new RobotMoveParameter(0, 0, 0)) + "\n" );
        }
        else
        {
            PropPos = () => gameObject.transform.position;
            PropForward = () => gameObject.transform.forward;
            PropRight = () => gameObject.transform.right;
            MoveAction = (para) =>
            {
                gameObject.transform.Translate(new Vector3(para.SpeedMMPS * Mathf.Cos(para.Rad), 0, para.SpeedMMPS * Mathf.Sin(para.Rad)) * 0.001f * 0.1f, Space.Self);
                gameObject.transform.Rotate(new Vector3(0, -para.Omega * Mathf.Rad2Deg, 0) * 0.1f);
            };
            StopAction = () => gameObject.transform.Translate(0, 0, 0, Space.World);
        }

        var pos = ConvertVec2(GetXZPlaneVec(PropPos()));
        _rvoIndex = RVO.Simulator.Instance.addAgent(pos);
    }

    public void MoveActuator(RobotMoveParameter para)
    {
        MoveAction(para);
    }

    public void Stop()
    {
        StopAction();
    }

    public RobotMoveParameter CalcParaWithRVO(Transform target)
    {
        //targetとpropの位置からrvoを使うかどうかなどの判断をする。
        var propPos                 = GetXZPlaneVec(PropPos());
        var targetPos               = GetXZPlaneVec(target.position);
        var vectorToTarget          = targetPos - propPos;
        var disBetweenPropAndTarget = vectorToTarget.magnitude;
        var nowRVOSimulationPos     = RVO.Simulator.Instance.getAgentPosition(_rvoIndex);
        var disBetweenRVOAndProp    = (ConvertVec2(nowRVOSimulationPos) - propPos).magnitude;

        if(disBetweenPropAndTarget < _RangePropGoToTargetDirectly)
        {
            //Propがtargetに近くならtargetに向かって進む

            RVO.Simulator.Instance.setAgentPosition(_rvoIndex, ConvertVec2(targetPos));
            //RVO.Simulator.Instance.setAgentPrefVelocity(_rvoIndex, ConvertVec2(vectorToTarget));

            return CalcPara(target);
        }
            
        if(disBetweenRVOAndProp < _RVOMovableRange)
        {
            //RVOに従って動く、RVOは動く
            RVO.Simulator.Instance.setAgentPrefVelocity(_rvoIndex, ConvertVec2(vectorToTarget));
        }
        else
        {
            //遠いなら、RVOシミュレータの動きを止める
            RVO.Simulator.Instance.setAgentPrefVelocity(_rvoIndex, new RVO.Vector2(0, 0));
        }

        RVO.Simulator.Instance.doStep();

        var RVOsimulatedPoint = RVO.Simulator.Instance.getAgentPosition(_rvoIndex);

        //トラッキングエリア外に出ないように結果を制限
        RVOsimulatedPoint = RestrictRange(RVOsimulatedPoint, 3.0f, 3.0f);
        RVO.Simulator.Instance.setAgentPosition(_rvoIndex, RVOsimulatedPoint);

        return CalcPara(target, ConvertVec2(RVOsimulatedPoint));

    }

    private RobotMoveParameter CalcPara(Transform target)
    {
        //目標地点までの移動を計算
        var VectorToTarget = GetXZPlaneVec(target.position) - GetXZPlaneVec(PropPos());
        var distanceToTarget = VectorToTarget.magnitude;

        int resultMoveSpeed = 0;
        float resultMoveVec = 0;

        var isDistanceOK = false;
        if (distanceToTarget > _distanceErrorThreshold)
        {
            //目標地点に移動するベクトルを求める
            resultMoveSpeed = CulclateMoveSpeed(VectorToTarget);
            resultMoveVec = Vector2.SignedAngle(GetXZPlaneVec(PropRight()), VectorToTarget);
        }
        else
        {
            _lastDistaneError = 0;
            isDistanceOK = true;
            Debug.Log("Distance OK");
        }
        
        //ロボットの回転を計算
        var targetFrontDirection = GetXZPlaneVec(target.up).normalized;
        var robotFrontDirection = GetXZPlaneVec(PropForward()).normalized;
        var innerProduct = Vector2.Dot(robotFrontDirection, targetFrontDirection);
        targetFrontDirection = innerProduct >= 0 ? targetFrontDirection : GetXZPlaneVec(-target.up).normalized;

        float resultTurn = 0;
        var isAngleOK = false;
        if(Vector2.Angle(robotFrontDirection, targetFrontDirection) > _angleErrorThreshold)
        {
            //目標角度に回転するパラメータを求める
            resultTurn = CulclateTurn(robotFrontDirection, targetFrontDirection);
        }
        else
        {
            _lastAngleError = 0;
            isAngleOK = true;
            Debug.Log("Angle OK");
        }

        //配置済みかどうかを判定
        _isPlaced = isDistanceOK && isAngleOK;
        _lastCalcTime = Time.time;

        return new RobotMoveParameter(resultMoveSpeed, resultMoveVec * Mathf.Deg2Rad, resultTurn * Mathf.Deg2Rad);
    }

    private RobotMoveParameter CalcPara(Transform target, Vector2 RVOPos)
    {
        //目標地点までの移動を計算
        var VectorToTarget = RVOPos - GetXZPlaneVec(PropPos());
        var distanceToTarget = VectorToTarget.magnitude;

        int resultMoveSpeed = 0;
        float resultMoveVec = 0;
        
        if (distanceToTarget > _distanceErrorThreshold)
        {
            //目標地点に移動するベクトルを求める
            resultMoveSpeed = CulclateMoveSpeed(VectorToTarget);
            resultMoveVec = Vector2.SignedAngle(GetXZPlaneVec(PropRight()), VectorToTarget);
        }
        else
        {
            _lastDistaneError = 0;
        }

        //ロボットの回転を計算
        var targetFrontDirection = GetXZPlaneVec(target.up).normalized;
        var robotFrontDirection = GetXZPlaneVec(PropForward()).normalized;
        var innerProduct = Vector2.Dot(robotFrontDirection, targetFrontDirection);
        targetFrontDirection = innerProduct >= 0 ? targetFrontDirection : GetXZPlaneVec(-target.up).normalized;

        float resultTurn = 0;
        if (Vector2.Angle(robotFrontDirection, targetFrontDirection) > _angleErrorThreshold)
        {
            //目標角度に回転するパラメータを求める
            resultTurn = CulclateTurn(robotFrontDirection, targetFrontDirection);
        }
        else
        {
            _lastAngleError = 0;
        }

        _lastCalcTime = Time.time;
        return new RobotMoveParameter(resultMoveSpeed, resultMoveVec * Mathf.Deg2Rad, resultTurn * Mathf.Deg2Rad);
    }
 

    /// <summary>
    /// ロボットが目標地点に着くようにPID制御で調整する
    /// </summary>
    private int CulclateMoveSpeed(Vector2 toGoalVec)
    {
        //PIDに必要なエラー距離と速度とエラーの和を計算する
        var distanceError = toGoalVec.magnitude;
        var velocity = (_lastDistaneError - distanceError) / (Time.time - _lastCalcTime);
        _distanceErrorSum += (_lastDistaneError + distanceError) / 2 * (Time.time - _lastCalcTime);
        _lastDistaneError = distanceError;

        //PID制御により、移動速度を決める
        var PIDSpeed = _pOfDirection * distanceError + _iOfDirection * _distanceErrorSum - _dOfDirection * velocity;

        //最高速度で抑える
        PIDSpeed = PIDSpeed > _maxSpeed ? _maxSpeed : PIDSpeed;
        PIDSpeed = PIDSpeed < -1 * _maxSpeed ? -1 * _maxSpeed : PIDSpeed;

        return (int)PIDSpeed;
    }

    /// <summary>
    /// 現在ロボットが向いている方向をルンバの向きたい方向にPID制御で調整する
    /// </summary>
    private float CulclateTurn(Vector2 nowVec, Vector2 goalVec)
    {
        //外積を用いて、反時計回りのずれを負と定義する
        var outerProduct = -Cross(goalVec, nowVec);
        var AngleError = outerProduct >= 0 ? Vector2.Angle(nowVec, goalVec) : -Vector2.Angle(nowVec, goalVec);

        //PIDに必要な角速度とエラーの和を計算する
        var AngleVel = (_lastAngleError - AngleError) / (Time.time - _lastCalcTime);
        _angleErrorSum += (_lastAngleError + AngleError) / 2 * (Time.time - _lastCalcTime);
        _lastAngleError = AngleError;

        //PIDにより、角速度を決める
        var PIDAngularSpeed = _pOfAngle * AngleError + _iOfAngle * _angleErrorSum - _dOfAngle * AngleVel;


        //最高角速度で抑える
        PIDAngularSpeed = PIDAngularSpeed > _maxTurnSpeed ? _maxTurnSpeed : PIDAngularSpeed;
        PIDAngularSpeed = PIDAngularSpeed < -1 * _maxTurnSpeed ? -1 * _maxTurnSpeed : PIDAngularSpeed;

        return PIDAngularSpeed;
    }

    private void OnApplicationQuit()
    {
        StopAction();
    }
}
