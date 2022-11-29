using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayRealActuator : MonoBehaviour
{
    [SerializeField] private GameObject _body;
    [SerializeField] private GameObject _actuator;

    private float bodyYPos;
    private float actuYPos;

    void Awake()
    {
        bodyYPos = _body.transform.position.y;
        actuYPos = _actuator.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        DisplayPropPos();
    }

    private void DisplayPropPos()
    {
        _body.transform.position = new Vector3(gameObject.transform.position.x, bodyYPos, gameObject.transform.position.z);
        _actuator.transform.position = new Vector3(gameObject.transform.position.x, actuYPos, gameObject.transform.position.z);

        _body.transform.rotation = Quaternion.LookRotation(transform.up, _body.transform.up);
        _actuator.transform.rotation = Quaternion.LookRotation(transform.up, _actuator.transform.up);
    }
}
