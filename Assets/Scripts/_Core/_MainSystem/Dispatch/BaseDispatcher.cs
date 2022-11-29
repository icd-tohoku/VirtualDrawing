using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseDispatcher : MonoBehaviour
{
    public abstract void Dispatch(Prop[] props, List<Transform> goals);
}