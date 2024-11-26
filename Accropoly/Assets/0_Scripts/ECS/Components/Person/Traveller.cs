using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace Components
{
    public struct Traveller : IComponentData, IEnableableComponent
    {
        public int nextWaypointIndex;
        public UnsafeList<Waypoint> waypoints;
    }
}
public struct Waypoint
{
    public float2 pos;
}
namespace Tags
{
    public struct Travelling : IComponentData, IEnableableComponent { }
}