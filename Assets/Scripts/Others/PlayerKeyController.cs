using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKeyController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 0.4f;
    [SerializeField] private float _roleSpeed = 50f;

    void Update()
    {
        MoveUpdate();
    }

    private void MoveUpdate()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.position += transform.forward * _moveSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.position += -transform.forward * _moveSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(0, 1 * _roleSpeed * Time.deltaTime, 0);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(0, -1 * _roleSpeed * Time.deltaTime, 0);
        }
    }
}
