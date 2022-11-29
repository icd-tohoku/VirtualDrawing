using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemEvaluationSystem : MonoBehaviour
{
    [SerializeField] private Prop _prop = null;
    [SerializeField] GameObject _userObj = null;
    [SerializeField] private GameObject _target = null;


    public enum Distance
    {
        longDis,
        middleDis,
        shortDis,
    }
    [Header("TargetConditions")]
    [SerializeField] private Distance _distanceType = Distance.longDis;
    public enum Height
    {
        highHeight,
        middleHeight,
        lowHeight,
    }
    [SerializeField] private Height _heightType = Height.highHeight;

    public enum Angle
    {
        minus0,
        minus45,
        minus90,
        minus180,
    }
    [SerializeField] private Angle _angleType = Angle.minus0;

    
    public enum Speed
    {
        high08,
        middle06,
        slow04,
    }
    [Header("UserConditions")]
    [SerializeField] private Speed _speedType = Speed.high08;
    private float _userSpeed = 0.4f;

    private bool _isStartEvaluation = false;
    private bool _isuserReached = false;
    private bool _isPropReached = false;
    private bool _hasLogged = false;
    private float _delay = 0f;


    void Start()
    {
        float goalDistance = 0f;
        if(_distanceType == Distance.longDis)
        {
            goalDistance = 1.5f;
        }
        else if(_distanceType == Distance.middleDis)
        {
            goalDistance = 0f;
        }
        else if (_distanceType == Distance.shortDis)
        {
            goalDistance = -1.5f;
        }
        
        float height = 1f;
        if(_heightType == Height.highHeight)
        {
            height = 1.4f;
        }
        else if (_heightType == Height.middleHeight)
        {
            height = 1.2f;
        }
        else if(_heightType == Height.lowHeight)
        {
            height = 1f;
        }

        _target.transform.position = new Vector3(goalDistance, height, 0);

        float plusAngle = 0;
        if(_angleType == Angle.minus0)
        {
            ;
        }
        else if (_angleType == Angle.minus45)
        {
            plusAngle = -45f;
        }
        else if(_angleType == Angle.minus90)
        {
            plusAngle = -90f;
        }
        else if (_angleType == Angle.minus180)
        {
            plusAngle = -180f;
        }
        _target.transform.rotation = Quaternion.Euler(_target.transform.rotation.eulerAngles.x, _target.transform.rotation.eulerAngles.y, _target.transform.rotation.eulerAngles.z + plusAngle);

        if (_speedType == Speed.high08)
        {
            _userSpeed = 0.8f;
        }
        else if (_speedType == Speed.middle06)
        {
            _userSpeed = 0.6f;
        }
        else if (_speedType == Speed.slow04)
        {
            _userSpeed = 0.4f;
        }

        _target.SetActive(false);
    }



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            _target.SetActive(true);
            _isStartEvaluation = true;
        }

        if (!_isStartEvaluation) return;

        MoveUser();

        CheckPropReach();

        CalcDelay();
    }

    private void MoveUser()
    {
        if (_isuserReached) return;

        _userObj.transform.Translate(Vector3.right * _userSpeed * Time.deltaTime);

        if (_userObj.transform.position.x > _target.transform.position.x)
        {
            _isuserReached = true;
        }
    }

    private void CheckPropReach()
    {
        if(_prop.propState == Prop.PropState.Placed)
        {
            _isPropReached = true;
        }
        else
        {
            _isPropReached = false;
        }
    }

    private void CalcDelay()
    {
        //delayの測定
        if (_isuserReached && _isPropReached)
        {
            //delayを出力
            if (!_hasLogged)
            {
                Debug.Log($"delay: {_delay}\nError: {_prop.Error}");
                _hasLogged = true;
            }
            
        }
        else if (_isuserReached && !_isPropReached)
        {
            _delay += Time.deltaTime;
        }
        else
        {
            _delay = 0f;
        }
    }
}
