using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static UnityEngine.GUILayout;

[CustomEditor(typeof(LineList))]
public class LineListEditor : Editor
{
    public override void OnInspectorGUI()
    {
        LineList linelist = target as LineList;
        base.OnInspectorGUI();
        if(GUILayout.Button("Add Element from Vectors"))
        {
            linelist.Add();
        }
        if (GUILayout.Button("TestDraw"))
        {
            linelist.ActionInvoke();
        }
        linelist.StartDrawingPoint = EditorGUILayout.Vector2Field("Start Draw Point", linelist.StartDrawingPoint);
        linelist.DotSize = EditorGUILayout.IntField("Dot Size",linelist.DotSize);
        if(GUILayout.Button("Dot Start"))
        {
            linelist.DotStartInvoke();
        }
        if (GUILayout.Button("Clear"))
        {
            linelist.Lines.Clear();
        }
    }
}
