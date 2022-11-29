using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DrawLines")]
public class LineList : ScriptableObject
{
    public List<LineParameter> Lines;

    [Space(50)]
    public Vector2 AddStart;
    public Vector2 AddEnd;

    [HideInInspector] public Vector2 StartDrawingPoint;
    [HideInInspector] public int DotSize = 5;

    public void Add() => Lines.Add(new LineParameter(AddStart, AddEnd));
    public void Add(Vector2 addStart,Vector2 addEnd) => Lines.Add(new LineParameter(addStart, addEnd));


    public event Action action;
    public void ActionInvoke() => action?.Invoke();

    public event Action DotStart;

    public void DotStartInvoke()
        => DotStart?.Invoke();
    
}
