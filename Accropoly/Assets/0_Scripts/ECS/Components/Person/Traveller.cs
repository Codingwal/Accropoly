using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using System;

namespace Components
{
    public struct Traveller : IComponentData
    {
        public int2 destination;
        public float timeOnTile;
        public int nextWaypointIndex;
        public UnsafeList<Waypoint> waypoints;
    }
}
public struct Waypoint : IEquatable<Waypoint>
{
    public int2 pos;
    public Waypoint(int2 pos)
    {
        this.pos = pos;
    }
    public bool Equals(Waypoint other)
    {
        return pos.Equals(other.pos);
    }
}
namespace Tags
{
    public struct Travelling : IComponentData, IEnableableComponent { }
    public struct WantsToTravel : IComponentData, IEnableableComponent { }
}