using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetDualDisplay : MonoBehaviour
{
    // イニシャライゼ―ションに使用
    void Start()
    {
        Debug.Log("displays connected: " + Display.displays.Length);
        // Display.displays[0] は主要デフォルトディスプレイで、常に ON。
        // 追加ディスプレイが可能かを確認し、それぞれをアクティベートします。
        if (Display.displays.Length > 1)
            Display.displays[1].Activate();

    }
    
}
