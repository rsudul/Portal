using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtility
{
    public static Quaternion ClampQuaternion(Quaternion q, float clampMin, float clampMax)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, clampMin, clampMax);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }
}