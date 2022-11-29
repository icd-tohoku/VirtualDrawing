using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class TargetControllerForDemo : BaseTargetDeterminer
{
    [SerializeField] private Transform[] _target;
    private int _targetIndex = 0;

    public SteamVR_ActionSet actionset;
    public SteamVR_Input_Sources handType;
    public SteamVR_Action_Boolean grab;

    private void Awake()
    {
        if(_target.Length < 1)
        {
            Debug.LogError("targetを入れてください");
            Debug.Break();
        }
    }

    void Update()
    {
        if (grab.GetStateDown(handType))
        {
            if (_targetIndex < _target.Length - 1)
            {
                _targetIndex++;
            }
            else
            {
                _targetIndex = 0;
            }
        }
    }

    public override List<Transform> FindTarget(Transform userPos)
    {
        List<Transform> targets = new List<Transform>();
        targets.Add(_target[_targetIndex]);
        return targets;
    }
}
