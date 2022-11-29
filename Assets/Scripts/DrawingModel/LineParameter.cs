using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// y = ax + b を表します．
/// isVertical = trueの時，x = aを表します．
/// </summary>
[System.Serializable]
public class LineParameter
{
    public float a;
    public float b;
    public float MinX;
    public float MaxX;

    public bool isVertical;
    public float MinY;
    public float MaxY;

    public float CalculateX(float y)
        => a == 0 ? 0 : (y - b) / a;

    public float CalculateY(float x)
        => a * x + b;

    public Vector2 StartPoint()
        => isVertical ? new Vector2(a,MinY) : new Vector2(MinX, CalculateY(MinX));

    public Vector2 EndPoint()
        => isVertical ? new Vector2(a, MaxY) : new Vector2(MaxX, CalculateY(MaxX));

    public override string ToString()
        => isVertical ? $"Vertical,{a},{MinY},{MaxY}" : $"NotVertical,{a},{b},{MinX},{MaxX}";

    public LineParameter(int atemp,int btemp,float mintemp,float maxtemp)
    {
        a = atemp;b = btemp;MinX = mintemp;MaxX = maxtemp;
        isVertical = false;MaxY = MinY = 0;
    }

    public LineParameter(Vector2 start, Vector2 end)
    {
        if(Mathf.Abs(end.x - start.x) < 1)
        {
            isVertical = true;
            a = start.x;
            MinY = start.y;
            MaxY = end.y;
            MinX = start.x;
            MaxX = end.x;
            b = 0;
        }
        else
        {
            MinX = start.x;
            MaxX = end.x;
            a = (end.y - start.y) / (end.x - start.x);
            b = start.y - a * start.x;
            isVertical = false; MaxY = MinY = 0;
        }

    }
}
