using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

/// <summary>
/// targetにつけるスクリプト
/// </summary>
public class PursuitTargetForDemo : MonoBehaviour
{
    public SteamVR_ActionSet actionset;
    public SteamVR_Input_Sources handType;
    public SteamVR_Action_Boolean grab;

    [SerializeField] private GameObject _user;

    [SerializeField] private float _distance = 0.7f;

    private enum State
    {
        state1,
        state2,
    }
    [SerializeField]private State state = State.state1;
    
    private void Update()
    {
        if (grab.GetStateDown(handType))
        {
            state = State.state2;
        }

        //positionを決める
        if(state == State.state1)
        {
            transform.position = new Vector3(_user.transform.position.x - _distance, 1.1f, _user.transform.position.z);
        }
        else if(state == State.state2)
        {
            transform.position = new Vector3(_user.transform.position.x - _distance, 1.2f, _user.transform.position.z);
        }

        //rotationを決める
        var direction = _user.transform.position - gameObject.transform.position;
        direction.y = 0;
        Quaternion lookRotation = new Quaternion();
        if (state == State.state1)
        {
            lookRotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.AngleAxis(90, Vector3.right) * Quaternion.AngleAxis(180, Vector3.up) * Quaternion.AngleAxis(-90, Vector3.forward);
        }
        else if (state == State.state2)
        {
            lookRotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.AngleAxis(90, Vector3.right) * Quaternion.AngleAxis(180, Vector3.up) * Quaternion.AngleAxis(30, Vector3.right);
        }
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, 1);
    }
}
