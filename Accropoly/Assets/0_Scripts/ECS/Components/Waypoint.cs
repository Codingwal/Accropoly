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
            public FixedFloat3Array5 next;
            public FixedFloat3Array5 previous;
            public bool exit; // Only important if at the tile's edge. false => entry
            public Connections(bool exit)
            {
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
