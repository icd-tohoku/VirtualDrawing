using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupObjects : MonoBehaviour
{
    [SerializeField] private List<GameObject> _objects;
    private int _index = 0;

    void Start()
    {
        if(_objects.Count >0)
        {
            foreach (GameObject obj in _objects)
            {
                obj.SetActive(false);
            }

            _objects[_index].SetActive(true);
        }
        else
        {
            Debug.LogError("消すオブジェクトがありません");
        }  
    }

    void Update()
    {
        if (_index >= _objects.Count -1 ) return;

        if (Input.GetKeyDown(KeyCode.P)) //"P"opUp
        {
            _objects[_index].SetActive(false);
            _index++;
            _objects[_index].SetActive(true);
        }   
    }
}
