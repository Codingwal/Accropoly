using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using System;

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
            public NewWaypoint(float3 a, float3 b, float3 c, float3 d)
            {
                nextWaypoints.Clear(float.NaN);
                nextWaypoints[0] = a;
                nextWaypoints[1] = b;
                nextWaypoints[2] = c;
                nextWaypoints[3] = d;
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
                Debug.LogError("Too many connections");
            }
        }
        public struct Waypoint : IComponentData // Always present
        {
            public TravelObjects allowedObjects;
            public float velocity; // the highest allowed velocity in m/s
            public Waypoint(TravelObjects allowedObjects, float velocity)
            {
                this.allowedObjects = allowedObjects;
                this.velocity = velocity;
            }
        }
        public struct Connections : IComponentData // Always present
        {
            public FixedEntityArray5 next;
            public FixedEntityArray5 previous;
            public bool entry; // Only important if at the tile's edge
            public bool exit; // Only important if at the tile's edge
            public Connections(bool entry, bool exit)
            {
                next.Clear(Entity.Null);
                previous.Clear(Entity.Null);
                this.entry = entry;
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


        /// <summary>
        /// A "bitmap" enum containing all forms of travel. <para/>
        /// Used to specify the allowed objects on a waypoint / the useable objects for a journey
        /// </summary>
        [Flags]
        public enum TravelObjects
        {
            None = 0,
            Car = 1 << 0,
            Pedestrian = 1 << 1,
            ServiceVehicle = 1 << 2, // Garbage vehicles, ...
            Truck = 1 << 3,
            EmergencyVehicle = 1 << 4,

            // Use these for waypoints
            Street = Car,
            Sidewalk = Pedestrian,
            PedestrianZone = Pedestrian | ServiceVehicle | EmergencyVehicle | Truck,

            // Use these to specify the allowed vehicles on a journey
            Standard = Pedestrian | Car, // For normal civilian journeys
        }
    }
}
