using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PQlab等で飛んできたRay入力を受ける方ののスクリプトにつける
/// </summary>
public abstract class RayInputSystem : MonoBehaviour
{
    public abstract void RayInteraction(RaycastHit hit);

    public abstract void NoTouch();
}
