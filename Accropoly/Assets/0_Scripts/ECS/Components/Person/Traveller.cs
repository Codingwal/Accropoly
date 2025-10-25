using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Components.WaypointComponents;
using Unity.Collections;

namespace Components
{
    public struct Traveller : IComponentData
    {
        // Journey info
        public int2 destination;
        public TravelObjects useableVehicles;

        public float maxAcceleration; // Constant

        // Path info
        public int nextWaypointIndex; // Used by movement system
        public UnsafeList<float3> waypoints; // Should not be serialized
        public float3 NextWaypoint => waypoints[nextWaypointIndex];

        public float3 velocity; // Used by movement system

        public void SetJourneyData(int2 destination, TravelObjects useableVehicles)
        {
            this.destination = destination;
            this.useableVehicles = useableVehicles;
        }

        /// <remarks>Does not reset journey info</remarks>
        public void Reset()
        {
            // Reset path info
            if (waypoints.IsCreated)
                waypoints.Clear();
            else
                waypoints = new(8, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            nextWaypointIndex = 0;
            velocity = 0;
            maxAcceleration = 10;
        }
    }

    // For serialization purposes
    public struct TravellerWaypointsSerializable : IComponentData
    {
        public UnsafeList<float3> waypoints;
    }
}
namespace Tags
{
    public struct Travelling : IComponentData, IEnableableComponent { }
    public struct WantsToTravel : IComponentData, IEnableableComponent { }
}