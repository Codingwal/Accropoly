using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

public struct WaypointsData : IComponentData
{
    public NativeHashMap<float3, Waypoint> waypoints;
}