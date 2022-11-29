using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingBox : MonoBehaviour
{
    private List<GameObject> _selectedObject = new List<GameObject>();
    public List<GameObject> SelectedObject
    {
        get
        {
            return _selectedObject;
        }
    }

    [SerializeField] private string objTag = "tag name";
    [SerializeField] private float _offset = 0.6f;
    [SerializeField] private GameObject _player = null;
    
    private void Awake()
    {
        try
        {            
            if(_player == null)
            {
                _player = GameObject.Find("[CameraRig]/Camera");
            }         
        }
        catch(NullReferenceException ex)
        {
            Debug.Break();
            throw ex;
        }
    }

    private void Update()
    {
        //BoundingBoxの位置
        var boundingBoxPos = _player.transform.position + _offset * _player.transform.forward;
        transform.position = new Vector3(boundingBoxPos.x, transform.position.y, boundingBoxPos.z);
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, _player.transform.eulerAngles.y, transform.eulerAngles.z);

        if (_selectedObject.Count == 0) return;

        //Activeじゃないオブジェクトは選択しない
        _selectedObject.RemoveAll(i => !i.activeInHierarchy);

        //距離の順に並び変える
        var temp = _selectedObject
            .OrderBy(i => Get2DDistance(_player, i))
            .ToList();
        _selectedObject.Clear();
        _selectedObject.AddRange(temp);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.activeSelf && other.gameObject.CompareTag(objTag))
        {
            _selectedObject.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_selectedObject.Contains(other.gameObject)) 
            _selectedObject.Remove(other.gameObject);
    }

    private float Get2DDistance(GameObject obj1, GameObject obj2)
    {
        var vec1 = new Vector2(obj1.transform.position.x, obj1.transform.position.z);
        var vec2 = new Vector2(obj2.transform.position.x, obj2.transform.position.z);
        return Vector2.Distance(vec1, vec2);
    }
}
