using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeterminerForDrawingPrac : BaseTargetDeterminer
{
    [SerializeField] private bool useProp = false;
    private BoundingBox _boundingBox;

    private void Awake()
    {
        try
        {
            _boundingBox = GameObject.Find("BoundingBox").GetComponent<BoundingBox>();
        }
        catch (NullReferenceException ex)
        {
            Debug.Break();
            throw ex;
        }
    }

    public override List<Transform> FindTarget(Transform userPos)
    {
        if (!useProp) return new List<Transform>();

        var goalList = _boundingBox.SelectedObject
            .Select(i => i.transform)
            .ToList();

        return goalList;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D)) //"D"eterminer
        {
            useProp = !useProp;
        }
    }
}
