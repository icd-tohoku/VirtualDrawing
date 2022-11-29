using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoundingBoxDeterminer : BaseTargetDeterminer
{
    private BoundingBox _boundingBox;

    private void Awake()
    {
        try
        {
            _boundingBox = GameObject.Find("BoundingBox").GetComponent<BoundingBox>();
        }
        catch(NullReferenceException ex)
        {
            Debug.Break();
            throw ex;
        }
    }

    public override List<Transform> FindTarget(Transform userPos)
    {
        var goalList = _boundingBox.SelectedObject
            .Select(i => i.transform)
            .ToList();

        return goalList;
    }
}
