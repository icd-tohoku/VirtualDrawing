using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasePathPlanner : MonoBehaviour
{
    public abstract void PathPlan(Transform userPos);
}

