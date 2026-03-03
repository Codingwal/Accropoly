using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace Components
{
    public struct WaypointsData : IComponentData
    {
        public NativeHashMap<float3, Entity> waypoints; // Used for fast lookup
    }

    namespace WaypointComponents
    {
        public struct NewWaypoint : IComponentData { }

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

        [InternalBufferCapacity(5)]
        public struct Connection : IBufferElementData // Always present
        {
            public float3 controlPoint;
            public float3 nextWaypoint;
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
