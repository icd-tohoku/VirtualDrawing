using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class TracingObjectSurface : MonoBehaviour
{
    public SteamVR_ActionSet actionset;
    public SteamVR_Input_Sources handType;
    public SteamVR_Action_Boolean grab;

    [SerializeField] private GameObject _tracecdObj;
    [SerializeField] private GameObject _target;


    void Update()
    {
        //スイッチの切り替え
        if (grab.GetState(handType))
        {
            //例をコントローラの下部から飛ばし、当たったところにターゲットを張り付ける
            RaycastHit hit;
            Ray ray = new Ray(this.transform.position, -this.transform.forward);
            if (Physics.Raycast(ray, out hit, 1f))
            {
                if (hit.collider.gameObject.name == _tracecdObj.name)
                {
                    _target.transform.position = hit.point;
                    _target.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                }
            }
        }
    }
}
