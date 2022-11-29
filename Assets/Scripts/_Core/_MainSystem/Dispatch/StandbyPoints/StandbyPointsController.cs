using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StandbyPointsController : MonoBehaviour
{
    private VRSurfaceMainSystem _vrSurfaceMainSystem;
    private GameObject _player;

    private void Start()
    {
        try
        {
            _vrSurfaceMainSystem = GameObject.Find("VRSurfaceMainSystem").GetComponent<VRSurfaceMainSystem>();
        }
        catch(NullReferenceException ex)
        {
            Debug.LogError(ex);
            Debug.Break();
        }

        _player = _vrSurfaceMainSystem.Player;
    }

    private void Update()
    {
        Vector3 standbyPointsPos = new Vector3(_player.transform.position.x, this.transform.position.y, _player.transform.position.z);
        this.transform.position = standbyPointsPos;

        Vector3 standbyPointRotation = new Vector3(this.transform.rotation.eulerAngles.x, _player.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z);
        this.transform.rotation = Quaternion.Euler(standbyPointRotation);
    }

}
