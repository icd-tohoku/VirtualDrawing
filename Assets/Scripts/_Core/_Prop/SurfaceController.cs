using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceController : MonoBehaviour
{
    private GameObject _surface;
    private Action<int, bool> SetAngle;

    [Header("シリアル通信")]
    [SerializeField] private bool _useSerial = false;
    [SerializeField] private string _portNum = "COM6";
    [SerializeField] private int _baudRate = 9600;
    private int _serialIndex;
    private Coroutine _surfaceCoroutine;

    private int _lastAngle = 30;
    private bool _lastIsForward = true;

    private Func<Vector3> PropForward;
    private Prop _prop;

    [SerializeField] private int offset = 0;

    private void Awake()
    {
        _prop = GetComponent<Prop>();
        
        if (_useSerial)
        {
            _serialIndex = SerialUtility.Instance.Open(_portNum, _baudRate);

            //angleの範囲は
            SetAngle = (angle, isForward) =>
            {
                //今回のサーボモータは0～180度の範囲で動くので、それに合わせてangleを変換する
                var survoAngleGoal = isForward ? 90 - angle - offset : 90 + angle - offset;
                if (survoAngleGoal > 0 && survoAngleGoal <= 180)
                {
                    SerialUtility.Instance.Writeln(_serialIndex, survoAngleGoal.ToString());
                }
                else if(survoAngleGoal > 180)
                {
                    survoAngleGoal = 180;
                    SerialUtility.Instance.Writeln(_serialIndex, survoAngleGoal.ToString());
                }
                else if(survoAngleGoal < 0)
                {
                    survoAngleGoal = 0;
                    SerialUtility.Instance.Writeln(_serialIndex, survoAngleGoal.ToString());
                }
            };

            PropForward = () => _prop.liftTopTracker.transform.up;

            //とりあえず最初は真上に向けておく
            SetAngle(30, true);
        }
        else
        {
            _surface = transform.Find("Surface").gameObject;
            SetAngle = (angle, isForward) =>
            {
                if(_surfaceCoroutine != null) StopCoroutine(_surfaceCoroutine);
                _surfaceCoroutine = StartCoroutine(RotateSimulation(angle, isForward));
            };
            PropForward = () => gameObject.transform.forward;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <returns>真上が0度で下に下がるほど大きくなり、真横で90度</returns>
    public int CalcSurfaceAngle(Transform target)
    {
        var targetNormalVec = target.transform.up;

        //targetが下方向を向いていたらSurfaceを回転させない
        //targetが真横を向いている時、法線ベクトルのy成分はなぜか-0.01になる。
        if (Math.Floor(targetNormalVec.y * 100) / 100f < -0.02f) return _lastAngle;

        //真上が0度で下に下がるほど大きくなり、真横で90度
        var angle = Mathf.RoundToInt( Vector3.Angle(targetNormalVec, new Vector3(0, 1, 0)));

        return angle;
    }

    /// <summary>
    /// angleの角度になるようにSurfaceを動かす。
    /// </summary>
    /// <param name="angle"></param>
    /// <param name="isForward">プロップとターゲットの向きが正方向に近い(true)か逆方向に近い(false)か</param>
    public void MoveSurface(int angle)
    {
        var isForward = ActuatorUtility.IsForword(PropForward(), _prop.Target);
        if (angle == _lastAngle && isForward == _lastIsForward) return;

        SetAngle(angle, isForward);

        _lastAngle = angle;
        _lastIsForward = isForward;
    }

    private IEnumerator RotateSimulation(int angle, bool isForward)
    {
        var simulateAngleGoal = isForward ? angle : -angle;　//-90～90度の範囲に設定
        var lerp = 0f;

        while (lerp < 1)
        {
            var lookRotation = Quaternion.Euler(new Vector3(simulateAngleGoal, 0, 0));
            _surface.transform.localRotation = Quaternion.Lerp(_surface.transform.localRotation, lookRotation, lerp);

            lerp += Time.deltaTime;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnApplicationQuit()
    {
        SetAngle(30, true);
    }
}
