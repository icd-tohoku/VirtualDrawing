using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class TrackerSmoothing
{
    [SerializeField]private Queue<Vector3> position = new Queue<Vector3>();
    [SerializeField]private Queue<Vector3> rotation = new Queue<Vector3>();
    public Vector3 AveragePos;
    public Vector3 AverageRot;
    [SerializeField] int count;

    public void Save(Vector3 pos, Vector3 rot)
    {
        position.Enqueue(pos);
        rotation.Enqueue(rot);
        if (position.Count > 20)
        {
            position.Dequeue();
        }
        if (rotation.Count > 20)
        {
            rotation.Dequeue();
        }

        count = position.Count;
        AveragePos = PositionSmooth();
        AverageRot = RotationSmooth();
    }

    private Vector3 PositionSmooth()
    {
        var pos = position.ToArray();
        return (pos.Aggregate(new Vector3(), (sum, acc) => sum += acc)) / pos.Length;
    }

    private Vector3 RotationSmooth()
    {
        var rot = rotation.ToArray();
        return (rot.Aggregate(new Vector3(), (sum, acc) => sum += acc)) / rot.Length;
    }
}
