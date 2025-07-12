using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace Components
{
    public struct Traveller : IComponentData
    {
        public int2 destination;
        public float3 velocity;
        public float maxAcceleration;
        public int nextWaypointIndex;
        public UnsafeList<Entity> waypoints; // Should not be serialized
        public Entity NextWaypoint => waypoints[nextWaypointIndex];
    }

    // For serialization purposes
    public struct TravellerWaypointsSerializable : IComponentData
    {
        public UnsafeList<float3> waypoints;
    }
}
namespace Tags
{
    public struct Travelling : IComponentData, IEnableableComponent { }
    public struct WantsToTravel : IComponentData, IEnableableComponent { }
}