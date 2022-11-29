using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TypingのTabletで、現実のtabletとVR内のKeyboardを合わせるクラス
//トラッカのupが、X・Z軸方向マイナスに向いていると、VRではちょっと奥(トラッカの-up方向)に表示される
public class TabletKeyBoardAjust : MonoBehaviour
{
    private GameObject _tabletTracker;

    [SerializeField] private float keyboardHightInXZPlus = 1f;
    [SerializeField] private float keyboardHightInXZMinus = -1f;
    void Start()
    {
        _tabletTracker = transform.root.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 keyboardLocalPos = new Vector3(0, 0, 0);

        var Xcomponent = Vector3.Dot(new Vector3(1, 0, 0), _tabletTracker.transform.forward);
        var Zcomponent = Vector3.Dot(new Vector3(0, 0, 1), _tabletTracker.transform.forward);

        /*        if(Xcomponent < -0.1f || Zcomponent < -0.1f)
                {
                    keyboardLocalPos.z = 2f;
                }
                else
                {
                    keyboardLocalPos.z = 0f;
                }*/

        //高さの調整
        var maxZ = keyboardHightInXZPlus;
        var minZ = keyboardHightInXZMinus;
        keyboardLocalPos.z = -(Xcomponent + Zcomponent) * (maxZ - minZ) / 2f + (maxZ + minZ) / 2f;

        transform.localPosition = keyboardLocalPos;
    }
}
