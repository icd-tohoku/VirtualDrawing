using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTabletColor : MonoBehaviour
{
    private Material _tabletMaterial;
    private GameObject _canvas = null;
    private float _threshold = 0.015f;

    private void Awake()
    {
        _tabletMaterial = GetComponent<Renderer>().material;
    }

    private void Update()
    {
        if (_canvas == null) return;

        if (_canvas.activeInHierarchy)
        {
            var tabletPos = transform.position;
            var canvasPos = _canvas.transform.position;


            var distance = Mathf.Abs( Vector3.Dot((tabletPos - canvasPos), _canvas.transform.up));
            
            if(distance < _threshold)
            {
                //近い(大体入力可能)
                _tabletMaterial.color = Color.blue;
                _tabletMaterial.color = _tabletMaterial.color - new Color32(0, 0, 0, 100);
            }
            else
            {
                //離れてる（大体入力不可）
                _tabletMaterial.color = Color.yellow;
                _tabletMaterial.color = _tabletMaterial.color - new Color32(0, 0, 0, 100);
            }
        }
        else
        {
            _canvas = null;
            _tabletMaterial.color = Color.green;
            _tabletMaterial.color = _tabletMaterial.color - new Color32(0, 0, 0, 100);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("DrawCanvas"))
        {
            _canvas = other.gameObject;
            Debug.Log("Find Canvas");
        }
    }
}
