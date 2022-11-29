using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePropColor : MonoBehaviour
{
    private Prop _prop;
    [SerializeField] private Material _propMaterial;

    private void Awake()
    {
        _prop = GameObject.Find("SimuratedProp").GetComponent<Prop>();
        
    }

    private void Update()
    {
        if(_prop.propState == Prop.PropState.dispatched)
        {
            _propMaterial.color = Color.yellow;
            _propMaterial.color = _propMaterial.color - new Color32(0, 0, 0, 100);
        }
        else if(_prop.propState == Prop.PropState.standby)
        {
            _propMaterial.color = Color.green;
            _propMaterial.color = _propMaterial.color - new Color32(0, 0, 0, 100);
        }
        else if (_prop.propState == Prop.PropState.Placed)
        {
            _propMaterial.color = Color.blue;
            _propMaterial.color = _propMaterial.color - new Color32(0, 0, 0, 100);
        }
    }
}
