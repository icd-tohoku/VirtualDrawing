using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MidAirKeyBoardPut : MonoBehaviour
{
    private BoundingBox _boundingBox;
    private GameObject _keyBoard;

    private void Awake()
    {
        try
        {
            _boundingBox = GameObject.Find("BoundingBox").GetComponent<BoundingBox>();
            _keyBoard = GameObject.Find("KeyBoard");
        }
        catch (NullReferenceException ex)
        {
            Debug.Break();
            throw ex;
        }
    }


    void Update()
    {
        var goalList = _boundingBox.SelectedObject
            .Select(i => i.transform)
            .ToList();

        if (goalList.Count > 0)
        {
            _keyBoard.SetActive(true);
            _keyBoard.transform.position = goalList[0].position;
            _keyBoard.transform.rotation = goalList[0].rotation;
        }
        else
        {
            _keyBoard.SetActive(false);
        }
    }
}
