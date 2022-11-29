using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetAutoMove : MonoBehaviour
{
    private float _radius = 1f;
    private float _runSpeed = 0.2f;
    private float _upDownSpeed = 0.5f;
    private float _angleSpeed = 0.1f;

    // Update is called once per frame
    void Update()
    {
        //XZ平面を円回転
        var x = _radius * Mathf.Sin(Time.time * _runSpeed);
        var z = _radius * Mathf.Cos(Time.time * _runSpeed);

        //Y軸方向に1.0~1.4mの間で上下
        var y = 1.2f + 0.2f * Mathf.Sin(Time.time * _upDownSpeed);

        transform.position = new Vector3(x, y, z);

        //角度をゆっくりと変える
        var angle = 60 * Mathf.Sin(Time.time * _angleSpeed);

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
