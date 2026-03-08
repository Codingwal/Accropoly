using Unity.Entities;

[Save("CurveFollower")]
public struct CurveFollower : IComponentData
{
    public BezierCurve curve;
    public float timeAlongCurve; // 0 - 1
}