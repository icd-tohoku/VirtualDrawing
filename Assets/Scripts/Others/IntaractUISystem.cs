using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


//Canvasとかにアタッチしておきたい
public class IntaractUISystem : RayInputSystem
{
    private Canvas _canvas;
    private IList<Graphic> _graphics;

    private void Start()
    {
        _canvas = gameObject.GetComponent<Canvas>();
        _graphics = GraphicRegistry.GetGraphicsForCanvas(_canvas);
    }

    public override void NoTouch()
    {
        throw new System.NotImplementedException();
    }

    public override void RayInteraction(RaycastHit hit)
    {
        if (hit.collider.gameObject != gameObject) return;

        for(int i = 0; i < _graphics.Count; ++i)
        {
            Graphic graphic = _graphics[i];

            if (graphic.depth == -1 || !graphic.raycastTarget)
            {
                continue;
            }


        }

        

        Debug.Log(hit.collider.gameObject.name);


        throw new System.NotImplementedException();

        //当たったのがボタンだったらInvoke


        //当たったのがスクロールバーだったら、そのRayhitの位置にスライド


        //当たったのがトグルだったらチェックボックスを変化


        //当たったのがスクロールビューだったら操作に合わせて画面を移動
    }


    public void ButtonClick1()
    {

    }

    public void SliderChanged()
    {

    }
}
