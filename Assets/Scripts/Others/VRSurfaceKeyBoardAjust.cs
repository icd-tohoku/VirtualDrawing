using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TypingのVRSurfaceで、現実のSurfaceとVR内のKeyboardを合わせるクラス
//トラッカのupが、X・Z軸方向マイナスに向いていると、VRではちょっと奥(トラッカの-up方向)に表示される
public class VRSurfaceKeyBoardAjust : MonoBehaviour
{
    private Vector3 BoardLocalPos;
    private Vector3 BoardLocalRot;
    private GameObject _surfaceTracker;

    [SerializeField] private float keyboardHightInXZPlus = 4.5f; 
    [SerializeField] private float keyboardHightInXZMinus = 0f; 

    // Start is called before the first frame update
    void Start()
    {
        _surfaceTracker = transform.root.gameObject;

        BoardLocalPos = transform.localPosition;
        BoardLocalRot = transform.localEulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 keyboardLocalPos;
        Vector3 keyboardLocalAng;
        bool isNomal = Vector3.Dot(Vector3.up, _surfaceTracker.transform.up) <= 0;
        if(isNomal)
        {
            //正方向
            keyboardLocalPos = BoardLocalPos;
            keyboardLocalAng = BoardLocalRot;
        }
        else
        {
            //逆方向
            Vector3 oppositPos = BoardLocalPos;
            oppositPos.y *= -1;
            keyboardLocalPos = oppositPos;

            Vector3 oppositRot = BoardLocalRot;           
            oppositRot.x += 180;
            oppositRot.y += 180;
            keyboardLocalAng = oppositRot;
        }

        //高さの調整
        var Xcomponent = Vector3.Dot(new Vector3(1, 0, 0), _surfaceTracker.transform.forward);
        var Zcomponent = Vector3.Dot(new Vector3(0, 0, 1), _surfaceTracker.transform.forward);
        //Zcomponent = Zcomponent < 0 ? Zcomponent : 0; 

        //minZ~maxZまでの関数
        var maxZ = keyboardHightInXZPlus;
        var minZ = keyboardHightInXZMinus;
        keyboardLocalPos.z = (Xcomponent + Zcomponent) * (maxZ-minZ) / 2f + (maxZ+minZ) / 2f;


        transform.localPosition = keyboardLocalPos;
        transform.localEulerAngles = keyboardLocalAng;
    }
}
