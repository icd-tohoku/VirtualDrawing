using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class Prop : MonoBehaviour
{   
    //このプロップが向かうべき目標地
    private Transform _target = null;
    public Transform Target
    {
        set
        {
            _target = value;
        }
        get
        {
            return _target;
        }
    }

    //このプロップの状態
    public enum PropState
    {
        dispatched,
        standby,
        Placed,
    }
    [HideInInspector] public PropState propState = PropState.standby;

    [Header("上下昇降の上のトラッカー")]
    public GameObject liftTopTracker = null;

    private bool _isSurfaceUsed = false;
    public bool IsSurfaceUsed
    {
        set
        {
            _isSurfaceUsed = value;
        }
        get
        {
            return _isSurfaceUsed;
        }
    }

    private float _error = 0f;
    /// <summary>
    /// _goalとLiftTrackerの誤差
    /// </summary>
    public float Error
    {
        get
        {
            return _error;
        }
    }
    //--------------------------------------------------------------

    //algorithmら
    private NexusRobotController _NexsusRobotController;
    private LiftController _liftController;
    private SurfaceController _surfaceController;

    private Transform _surface;
    private GameObject _goal;
    [SerializeField]private float _offset = 0.08f; //Surfaceとトラッカの高さの差を入れる
    private bool _isKilled = false;

    private float timer = 0;
    //--------------------------------------------------------------

    void Awake()
    {
        try
        {
            _NexsusRobotController = gameObject.GetComponent<NexusRobotController>();
            _liftController = gameObject.GetComponent<LiftController>();
            _surfaceController = gameObject.GetComponent<SurfaceController>();

            //_surface = transform.Find("Surface").gameObject;
            _goal = transform.Find("Goal").gameObject;//名前,後で考える
            _goal.transform.parent = null;

            _surface = transform.Find("Surface");

            if (liftTopTracker != null)
            {
                //Updateのタイミングでシミュレータの位置をトラッカに合わせる
                StartCoroutine(SetPositonToTraker());
            }
        }
        catch(System.Exception e)
        {
            Debug.LogError(e);
        } 
    }

    void Update()
    {
        //kill状態にするキー
        if (Input.GetKeyDown(KeyCode.K))
        {
            _isKilled = true;
        }

        if (liftTopTracker != null && liftTopTracker.transform.position == Vector3.zero)
        {
            Debug.LogError($"{liftTopTracker.name}のトラッカが認識されていません。");
            _NexsusRobotController.Stop();
            _liftController.Stop();
            return;
        }

        //以下の処理は0.1秒ごとに行う
        timer += Time.deltaTime;
        if (timer < 0.1f) return;
        timer = 0;


        //配置精度に限界があるため、前後のキー入力は受け付けるようにしておく
        var tempPara = new ActuatorUtility.RobotMoveParameter();
        if (Input.GetKey(KeyCode.UpArrow))
        {
            tempPara.SpeedMMPS = 30;
            tempPara.Rad = Mathf.Deg2Rad * Vector2.SignedAngle(new Vector2(0, -1), new Vector2(1, 0));
            _NexsusRobotController.MoveActuator(tempPara);
            return;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            tempPara.SpeedMMPS = 30;
            tempPara.Rad = Mathf.Deg2Rad * Vector2.SignedAngle(new Vector2(0, 1), new Vector2(1, 0));
            _NexsusRobotController.MoveActuator(tempPara);
            return;
        }

        //これら条件の時にはプロップは動かない
        if (_isKilled || _target == null || _isSurfaceUsed)
        {
            //実機を止める
            _NexsusRobotController.Stop();
            _liftController.Stop();
            propState = PropState.Placed;
            return;
        }

        //Goalの設定
        _goal.transform.position = _target.position + (-1) * _target.up * _offset + new Vector3(0, 0.01f, 0);//要調整
        _goal.transform.rotation = _target.transform.rotation;

        //NexusRobotを動かす
        var para = _NexsusRobotController.CalcParaWithRVO(_goal.transform);
        _NexsusRobotController.MoveActuator(para);

        //standby状態であればXZ平面の移動のみ
        if (propState == PropState.standby)return;

        //liftを動かす
        var liftMotion = _liftController.CalcLiftMotion(_goal.transform);
        _liftController.MoveLift(liftMotion);

        //Surfaceを動かす
        var surfaceAngle = _surfaceController.CalcSurfaceAngle(_goal.transform);
        _surfaceController.MoveSurface(surfaceAngle);

        //配置されていたらPlaced状態にする
        if(_NexsusRobotController.IsPlaced && _liftController.IsPlaced)
        {
            _error = CalcError();
            propState = PropState.Placed;
        }
        else
        {
            propState = PropState.dispatched;
        }
    }

    private IEnumerator SetPositonToTraker()
    {
        int nframe = 5;
        for (var i = 0; i < nframe; i++)
        {
            yield return null; //nフレーム待つ
        }

        transform.position = new Vector3(liftTopTracker.transform.position.x, transform.position.y, liftTopTracker.transform.position.z);
        transform.eulerAngles = new Vector3(0, liftTopTracker.transform.rotation.eulerAngles.y + 180f, 0);
    }

    private float CalcError()
    {
        if (_target == null)
        {
            return 0f;
        }
        else if(liftTopTracker != null)
        {
            return (_goal.transform.position - liftTopTracker.transform.position).magnitude;
        }
        else
        {
            return (_goal.transform.position - _surface.transform.position).magnitude;
        }
    }
}
