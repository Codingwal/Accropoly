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
        public UnsafeList<float3> waypoints;
    }
}
public unsafe struct Waypoint
{
    public float3 pos; // world space
    public float velocity; // in m/s
    public bool stop; // Should object stop at this waypoint? (managed by JunctionSystem)
    public int registeredObjects; // Count of objects that are currently moving to this waypoint
    public JunctionData junctionData;
    public bool exit; // Only important if at the tile's edge. false => entry
    public FixedFloat3Array5 next;
    public FixedFloat3Array5 previous;
    public Waypoint(float3 pos, float velocity, JunctionData junctionData, bool exit)
    {
        this.pos = pos;
        this.velocity = velocity;
        stop = false;
        registeredObjects = 0;
        this.junctionData = junctionData;
        this.exit = exit;

        next.Clear(float.NaN);
        previous.Clear(float.NaN);
    }
    public void RemoveNext(float3 pos)
    {
        for (int i = 0; i < next.Size; i++)
            if (next[i].Equals(pos))
                next[i] = float.NaN;
    }
    public void RemovePrevious(float3 pos)
    {
        for (int i = 0; i < previous.Size; i++)
            if (previous[i].Equals(pos))
                previous[i] = float.NaN;
    }
    public void AddNext(float3 pos)
    {
        for (int i = 0; i < next.Size; i++)
        {
            if (math.isnan(next[i].x))
            {
                next[i] = pos;
                return;
            }
        }
        Debug.LogError("No slot left");
    }
    public void AddPrevious(float3 pos)
    {
        for (int i = 0; i < previous.Size; i++)
        {
            if (math.isnan(previous[i].x))
            {
                previous[i] = pos;
                return;
            }
        }
        Debug.LogError("No slot left");
    }

    public enum JunctionData
    {
        None, // Not part of the junction
        Default,
        Priority,
        GiveWay,
    }
}
namespace Tags
{
    public struct Travelling : IComponentData, IEnableableComponent { }
    public struct WantsToTravel : IComponentData, IEnableableComponent { }
}