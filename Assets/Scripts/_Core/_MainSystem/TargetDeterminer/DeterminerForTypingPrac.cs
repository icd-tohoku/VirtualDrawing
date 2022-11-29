using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeterminerForTypingPrac : BaseTargetDeterminer
{
    [SerializeField] private bool useProp = false;
    [SerializeField] private GameObject _keyBoard;
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
        var goalList = _boundingBox.SelectedObject
            .Select(i => i.transform)
            .ToList();

        if (goalList.Count != 0 && !useProp)
        {
            //キーボードを一番近い付箋に貼る
            _keyBoard.SetActive(true);
            _keyBoard.transform.position = goalList[0].position;
            _keyBoard.transform.rotation = goalList[0].rotation;
            return new List<Transform>();
        }
        else
        {
            _keyBoard.SetActive(false);
        }

        return goalList;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D)) //"D"eterminer
        {
            useProp = !useProp;
        }
    }
}
