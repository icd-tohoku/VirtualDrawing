using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PQLabTouchController : MonoBehaviour
{    
    [SerializeField] private GameObject _touchPanel;
    [SerializeField] private RayInputSystem rayInput;
    [SerializeField] LayerMask layerMask;
    private Vector3 _screenSize;

    void Awake()
    {
        if (rayInput == null)
        {
            Debug.Break();
            throw new System.NullReferenceException("set pqLabInput");
        }
        _screenSize = new Vector3(Screen.width, Screen.height, 0);
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            //touchCountが0のときに呼ばれるとエラーでます
            //このフレームでのタッチ情報を取得
            Touch[] myTouches = Input.touches;

            //検出されているタッチの数分計算
            for (int i = 0; i < myTouches.Length; i++)
            {
                var PQlabTouchPosition = ProjectTouchOntoObjectWorldVEctor(myTouches[i]);

                //タッチ位置からパネルの法線方向にRayを飛ばして、当たった座標をとる
                RaycastHit hit;
                Ray ray = new Ray(PQlabTouchPosition, this.transform.up);
                if (Physics.Raycast(ray, out hit, 0.1f, layerMask))
                {
                    //当たった三次元座標を取得
                    rayInput.RayInteraction(hit);
                    return;
                }

                //さっきとは逆方向にRayを飛ばして、当たった座標をとる
                hit = new RaycastHit();
                ray = new Ray(PQlabTouchPosition, -1 * this.transform.up);
                if (Physics.Raycast(ray, out hit, 0.1f, layerMask))
                {
                    //当たった三次元座標を取得
                    rayInput.RayInteraction(hit);
                    return;
                }
            }
        }
    }

    private Vector3 ProjectTouchOntoObjectWorldVEctor(Touch touch)
    {
        //ローカル座標でのタッチ位置を計算
        var x = touch.position.x / _screenSize.x;
        var z = touch.position.y / _screenSize.y;
        var localPos = new Vector3(- 0.5f + x, 0f, - 0.5f + z);

        //ローカル座標からワールド座標に変換
        var worldPos = _touchPanel.transform.TransformPoint(localPos);

        return worldPos;
    }
}
