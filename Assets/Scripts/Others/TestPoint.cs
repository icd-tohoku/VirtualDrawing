using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPoint : RayInputSystem
{
    [SerializeField] GameObject _map;
    [SerializeField] GameObject _pin;


    public override void NoTouch()
    {
        throw new System.NotImplementedException();
    }

    public override void RayInteraction(RaycastHit hit)
    {
        if(hit.collider.gameObject == _map)
        {
            _pin.transform.position = hit.point;
        }
    }

}
