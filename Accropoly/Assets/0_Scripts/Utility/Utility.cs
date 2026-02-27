
using Unity.Mathematics;
using UnityEngine;

public static class Utility
{
    public static void Gizmo_DrawBezierCurve(float3 start, float3 control, float3 dest, int segmentCount)
    {
        // Handle straight lines (majority)
        if (control.Equals(dest))
        {
            Gizmos.DrawLine(start, dest);
            return;
        }

        float3 pos = start;
        float deltaTime = (float)1 / segmentCount;
        for (float t = deltaTime; t <= 1 + deltaTime / 2; t += deltaTime) // Set limit a bit higher to compensate of floating-point error
        {
            float3 nextPos = BezierCurve(start, control, dest, t);
            Gizmos.DrawLine(pos, nextPos);
            pos = nextPos;
        }
    }

    public static float3 BezierCurve(float3 start, float3 control, float3 dest, float t)
    {
        float3 a = Lerp(start, control, t);
        float3 b = Lerp(control, dest, t);
        return Lerp(a, b, t);
    }

    public static float3 Lerp(float3 a, float3 b, float t)
    {
        return a * (1 - t) + b * t;
    }
}