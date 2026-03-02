using Unity.Entities;

public struct CurveFollower : IComponentData
{
    public BezierCurve curve;
    public float timeAlongCurve; // 0 - 1
}