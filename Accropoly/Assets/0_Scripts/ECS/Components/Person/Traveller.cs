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
    public float3 pos;
    public float velocity;
    public FixedFloat3Array5 next;
    public FixedFloat3Array5 previous;
    public bool exit; // Only important if at the tile's edge. false => entry
    public Waypoint(float3 pos, float velocity, bool exit)
    {
        this.pos = pos;
        this.velocity = velocity;

        next.Clear(float.NaN);
        previous.Clear(float.NaN);

        this.exit = exit;
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
}
namespace Tags
{
    public struct Travelling : IComponentData, IEnableableComponent { }
    public struct WantsToTravel : IComponentData, IEnableableComponent { }
}