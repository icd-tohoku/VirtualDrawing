using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseTargetDeterminer : MonoBehaviour
{
    public abstract List<Transform> FindTarget(Transform userPos);
}