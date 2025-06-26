using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

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
public unsafe struct Waypoint
{
    public float3 pos;
    public FixedFloat3Array10 next;
    public FixedFloat3Array10 previous;
    public Waypoint(float3 pos)
    {
        this.pos = pos;

        next.Initialize(float.NaN);
        previous.Initialize(float.NaN);
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