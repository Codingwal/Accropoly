using Unity.Mathematics;
using UnityEngine;

public struct BezierCurve
{
    public float3 start;
    public float3 control;
    public float3 dest;

    public BezierCurve(float3 start, float3 control, float3 dest)
    {
        this.start = start;
        this.control = control;
        this.dest = dest;
    }

    public readonly bool IsLine => control.Equals(dest);
    public readonly void Draw(int segmentCount)
    {
        // Handle straight lines (majority)
        if (IsLine)
        {
            Gizmos.DrawLine(start, dest);
            return;
        }

        float3 pos = start;
        float deltaTime = (float)1 / segmentCount;
        for (float t = deltaTime; t <= 1 + deltaTime / 2; t += deltaTime) // Set limit a bit higher to compensate of floating-point error
        {
            float3 nextPos = GetPoint(t);
            Gizmos.DrawLine(pos, nextPos);
            pos = nextPos;
        }
    }

    public readonly float3 GetPoint(float t)
    {
        float oneMinusT = 1f - t;

        return math.square(oneMinusT) * start + 2 * oneMinusT * t * control + math.square(t) * dest;
    }
    public readonly float3 GetVelocity(float t)
    {
        return 2 * (1f - t) * (control - start) + 2 * t * (dest - control);
    }

    /// <summary>
    /// Approximate the delta time to move a certain distance along the curve
    /// </summary>
    public float GetDeltaTime(float currentTime, float distance)
    {
        float speed = math.length(GetVelocity(currentTime));
        return distance / speed;
    }
}