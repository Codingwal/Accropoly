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
    public const int maxConnections = 5;
    public float3 pos;
    public fixed int next[maxConnections];
    public fixed int previous[maxConnections];
    public Waypoint(float3 pos)
    {
        this.pos = pos;

        for (int i = 0; i < maxConnections; i++)
            next[i] = -1;

        for (int i = 0; i < maxConnections; i++)
            previous[i] = -1;
    }
    public void UpdateNext(int index, int newIndex)
    {
        for (int i = 0; i < maxConnections; i++)
            if (next[i] == index)
                next[i] = newIndex;
    }
    public void UpdatePrevious(int index, int newIndex)
    {
        for (int i = 0; i < maxConnections; i++)
            if (previous[i] == index)
                previous[i] = newIndex;
    }
    public void AddNext(int index)
    {
        for (int i = 0; i < maxConnections; i++)
        {
            if (next[i] == -1)
            {
                next[i] = index;
                return;
            }
        }
        Debug.LogError("No slot left");
    }
    public void AddPrevious(int index)
    {
        for (int i = 0; i < maxConnections; i++)
        {
            if (previous[i] == -1)
            {
                previous[i] = index;
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