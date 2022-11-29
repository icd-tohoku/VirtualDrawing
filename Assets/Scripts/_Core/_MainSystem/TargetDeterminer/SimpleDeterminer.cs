using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDeterminer : BaseTargetDeterminer
{   
    [SerializeField] private GameObject _target = null;
 
    void Start()
    {
        if (_target == null)
        {
            Debug.Break();
            throw new System.NullReferenceException("set target");
        }
    }

    public override List<Transform> FindTarget(Transform player)
    {
        List<Transform> list = new List<Transform>();
        if (_target.activeSelf)
        {
            list.Add(_target.transform);
        }
        return list;
    }
}
