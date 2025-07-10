using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Components
{
    public struct WaypointsData : IComponentData
    {
        public NativeHashMap<float3, Entity> waypoints; // Used for fast lookup
    }

    namespace WaypointComponents
    {
        public struct NewWaypoint : IComponentData
        {
            public FixedFloat3Array5 nextWaypoints;
            public NewWaypoint(float3 a)
            {
                nextWaypoints.Clear(float.NaN);
                nextWaypoints[0] = a;
            }
            public NewWaypoint(float3 a, float3 b)
            {
                nextWaypoints.Clear(float.NaN);
                nextWaypoints[0] = a;
                nextWaypoints[1] = b;
            }
            public NewWaypoint(float3 a, float3 b, float3 c)
            {
                nextWaypoints.Clear(float.NaN);
                nextWaypoints[0] = a;
                nextWaypoints[1] = b;
                nextWaypoints[2] = c;
            }
            public static NewWaypoint Default
            {
                get
                {
                    NewWaypoint tmp = new();
                    tmp.nextWaypoints.Clear(float.NaN);
                    return tmp;
                }
            }
            public void AddNext(float3 value)
            {
                for (int i = 0; i < nextWaypoints.Size; i++)
                {
                    if (!math.isnan(nextWaypoints[i].x)) continue;
                    nextWaypoints[i] = value;
                    return;
                }
            }
        }
        public struct Waypoint : IComponentData // Always present
        {
            public float velocity; // in m/s
            public Waypoint(float velocity)
            {
                this.velocity = velocity;
            }
        }
        public struct Connections : IComponentData // Always present
        {
            public FixedEntityArray5 next;
            public FixedEntityArray5 previous;
            public bool exit; // Only important if at the tile's edge. false => entry
            public Connections(bool exit)
            {
                next.Clear(Entity.Null);
                previous.Clear(Entity.Null);
                this.exit = exit;
            }
            public void RemoveNext(Entity entity)
            {
                for (int i = 0; i < next.Size; i++)
                    if (next[i].Equals(entity))
                        next[i] = Entity.Null;
            }
            public void RemovePrevious(Entity entity)
            {
                for (int i = 0; i < previous.Size; i++)
                    if (previous[i].Equals(entity))
                        previous[i] = Entity.Null;
            }
            public void AddNext(Entity entity)
            {
                for (int i = 0; i < next.Size; i++)
                {
                    if (next[i] == Entity.Null)
                    {
                        next[i] = entity;
                        return;
                    }
                }
                Debug.LogError("No slot left");
            }
            public void AddPrevious(Entity entity)
            {
                for (int i = 0; i < previous.Size; i++)
                {
                    if (previous[i] == Entity.Null)
                    {
                        previous[i] = entity;
                        return;
                    }
                }
                Debug.LogError("No slot left");
            }
        }
        public struct Junction : IComponentData // Optional
        {
            public bool stop; // Should object stop at this waypoint? (managed by JunctionSystem)
            public int registeredObjects; // Count of objects that are currently moving to this waypoint
            public JunctionData junctionData;
            public Junction(JunctionData junctionData)
            {
                stop = false;
                registeredObjects = 0;
                this.junctionData = junctionData;
            }

            public enum JunctionData
            {
                None, // Not part of the junction
                Default,
                Priority,
                GiveWay,
            }
        }
    }
}
