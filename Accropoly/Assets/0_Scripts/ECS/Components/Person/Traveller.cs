using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace Components
{
    public struct Traveller : IComponentData
    {
        public int2 destination;
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
    public struct WantsToTravel : IComponentData, IEnableableComponent { }
}