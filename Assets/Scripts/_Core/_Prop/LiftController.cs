using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftController : MonoBehaviour
{
    public enum LiftState
    {
        up,
        down,
        stop
    }
    private LiftState _nowLiftState = LiftState.stop;

    private Action UpEvent;
    private Action DownEvent;
    private Action StopEvent;
    private Func<Vector3> GetSurfacePos;
    private Coroutine _liftCoroutine;

    private GameObject _tracker;
    private GameObject _surface;

    [Header("制御パラメータ")]
    [SerializeField] private float _liftSpeed = 0.035f; //単位はm/s
    [SerializeField] private float _threshold = 0.001f; //単位はm
    [SerializeField] private float _maxHight = 2f;
    [SerializeField] private float _minHight = 0.9f;

    [Header("シリアル通信")]
    [SerializeField] private bool   _useSerial = false;
    [SerializeField] private string _portNum = "COM6";
    [SerializeField] private int    _baudRate = 9600;
    private int _serialIndex;

    private bool _isPlaced = false;
    public bool IsPlaced
    {
        get
        {
            return _isPlaced;
        }
    }

    private void Awake()
    {
        if (_useSerial)
        {
            _tracker = gameObject.GetComponent<Prop>().liftTopTracker;
            _serialIndex = SerialUtility.Instance.Open(_portNum, _baudRate);

            UpEvent = () => SerialUtility.Instance.Writeln(_serialIndex, "up");
            DownEvent = () => SerialUtility.Instance.Writeln(_serialIndex, "down");
            StopEvent = () => SerialUtility.Instance.Writeln(_serialIndex, "stop");
            GetSurfacePos = () => _tracker.transform.position;
        }
        else
        {
            _surface = transform.Find("Surface").gameObject;
            UpEvent = () =>
            {
                if(_liftCoroutine != null) StopCoroutine(_liftCoroutine);
                _liftCoroutine = StartCoroutine(SimulationUp());
            };
            DownEvent = () =>
            {
                if (_liftCoroutine != null) StopCoroutine(_liftCoroutine);
                _liftCoroutine = StartCoroutine(SimulationDown());
            };
            StopEvent = () =>
            {
                if (_liftCoroutine != null) StopCoroutine(_liftCoroutine);
                _surface.transform.Translate(0, 0, 0, Space.World);
            };
            GetSurfacePos = () => _surface.transform.position;
        }
    }

    public LiftState CalcLiftMotion(Transform target)
    {
        Vector3 nowSurfacePos = GetSurfacePos();

        var toTargetVec = target.position - nowSurfacePos;
        var hightDistance = Mathf.Abs(toTargetVec.y);

        if(hightDistance < _threshold)
        {
            return LiftState.stop;
        }
        else if (toTargetVec.y > 0 && nowSurfacePos.y <= _maxHight)
        {
            return LiftState.up;
        }
        else if (toTargetVec.y < 0 && nowSurfacePos.y >= _minHight)
        {
            return LiftState.down;
        }
        else
        {
            Debug.LogWarning("想定外の値が入力されました");
            return LiftState.stop;
        }
    }

    public void MoveLift(LiftState liftState)
    {
        switch (liftState)
        {
            case LiftState.up:
                _isPlaced = false;
                if (_nowLiftState == LiftState.up) return;
                _nowLiftState = LiftState.up;
                UpEvent();
                break;

            case LiftState.down:
                _isPlaced = false;
                if (_nowLiftState == LiftState.down) return;
                _nowLiftState = LiftState.down;
                DownEvent();              
                break;

            case LiftState.stop:
                _isPlaced = true;
                if (_nowLiftState == LiftState.stop) return;
                _nowLiftState = LiftState.stop;
                StopEvent();   
                break;
            default:
                break;
        }
    }

    public void Stop()
    {
        MoveLift(LiftState.stop);
    }

    private IEnumerator SimulationUp()
    {
        while(_surface.transform.position.y < _maxHight)
        {
            _surface.transform.Translate(0, _liftSpeed * Time.deltaTime, 0, Space.World);
            yield return null;
        }
    }

    private IEnumerator SimulationDown()
    {
        while (_surface.transform.position.y > _minHight)
        {
            _surface.transform.Translate(0, -1 * _liftSpeed * Time.deltaTime, 0, Space.World);
            yield return null;
        }
    }

    private void OnApplicationQuit()
    {
        StopEvent();
    }
}
