using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Components
{
    public struct Traveller : IComponentData
    {
        public int2 destination;
        public float3 velocity;
        public float maxAcceleration;
        public int nextWaypointIndex;
        public UnsafeList<Entity> waypoints;
        public Entity NextWaypoint => waypoints[nextWaypointIndex];
    }
}
namespace Tags
{
    public struct Travelling : IComponentData, IEnableableComponent { }
    public struct WantsToTravel : IComponentData, IEnableableComponent { }
}