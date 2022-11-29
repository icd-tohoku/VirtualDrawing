using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandbyPointSample : MonoBehaviour
{
    [SerializeField] private GameObject _user;
    [SerializeField] private float length = 1f;
    
    void Update()
    {
        var userForward = _user.transform.forward;
        userForward.y = 0;
        userForward = userForward.normalized * length;

        var standbyPos = new Vector3(_user.transform.position.x + userForward.x, transform.position.y, _user.transform.position.z + userForward.z);
       
        //常にユーザの前にスタンバイポイントを配置する
        transform.position = standbyPos;


        var direction = _user.transform.position - gameObject.transform.position;
        direction.y = 0;

        var lookRotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.AngleAxis(90, Vector3.right) * Quaternion.AngleAxis(180, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, 1);
    }
}
