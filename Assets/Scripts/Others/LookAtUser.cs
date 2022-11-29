using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// スタンバイポイントがユーザをみるように作った関数
/// </summary>
public class LookAtUser : MonoBehaviour
{
    [SerializeField] GameObject user;

    // Update is called once per frame
    void Update()
    {
        var direction = user.transform.position - gameObject.transform.position;
        direction.y = 0;

        var lookRotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.AngleAxis(90, Vector3.right) * Quaternion.AngleAxis(180, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, 1);
    }
}
