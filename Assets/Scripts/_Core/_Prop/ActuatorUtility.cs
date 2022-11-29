using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ActuatorUtility
{
    public  static Vector2 GetXZPlanePos(GameObject obj)
    {
        Vector2 vec = new Vector2(obj.transform.position.x, obj.transform.position.z);
        return vec;
    }

    public static Vector2 GetXZPlaneVec(Vector3 vec3)
    {
        Vector2 vec2 = new Vector2(vec3.x, vec3.z);
        return vec2;
    }

    public  static float Cross(Vector2 lhs, Vector2 rhs)
    {
        return lhs.x * rhs.y - lhs.y * rhs.x;
    }

    public static RVO.Vector2 ConvertVec2(UnityEngine.Vector2 uniVec)
    {
        return new RVO.Vector2(uniVec.x, uniVec.y);
    }

    public static UnityEngine.Vector2 ConvertVec2(RVO.Vector2 uniVec)
    {
        return new UnityEngine.Vector2(uniVec.x_, uniVec.y_);
    }

    /// <summary>
    /// シミュレータで得た位置を、トラッキングエリア内に抑える
    /// </summary>
    /// <param name="propPos"></param>
    public static RVO.Vector2 RestrictRange(RVO.Vector2 propPos,float areaX, float areaY)
    {
        if (propPos.x_ > areaX / 2f)
        {
            propPos.x_ = areaX / 2f;
        }
        else if (propPos.x_ < -areaX / 2f)
        {
            propPos.x_ = -areaX / 2f;
        }

        if (propPos.y_ > areaY / 2f)
        {
            propPos.y_ = areaY / 2f;
        }
        else if (propPos.y_ < -areaY / 2f)
        {
            propPos.y_ = -areaY / 2f;
        }
        return propPos;
    }

    /// <summary>
    /// プロップとターゲットの向きが正方向に近い(true)か逆方向に近い(false)かを返す
    /// </summary>
    public static  bool IsForword(Vector3 propForward, Transform target)
    {
        var innerProduct = Vector2.Dot(GetXZPlaneVec(propForward).normalized, GetXZPlaneVec(target.up).normalized);
        return innerProduct >= 0;
    }

    public struct RobotMoveParameter
    {
        public int SpeedMMPS;
        public float Rad;
        public float Omega;

        public RobotMoveParameter(int speedMMSP, float rad,float omega)
        {
            SpeedMMPS = speedMMSP;
            Rad = rad;
            Omega = omega;
        }
        public override string ToString()
            => $"speedMMOS:{SpeedMMPS}, rad:{Rad}, omega:{Omega}";
    }

    public static string FormatSetCarMove(RobotMoveParameter para)
        => $"Header\nSCM\n{para.SpeedMMPS}\n{Math.Round(para.Rad, 5, MidpointRounding.AwayFromZero)}\n{Math.Round(para.Omega, 5, MidpointRounding.AwayFromZero)}\n";

    public static string FormatStop()
        => "Header\nStop\n";
}
