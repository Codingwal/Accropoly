using Unity.Entities;
using Unity.Mathematics;
using Components.WaypointComponents;

namespace Components
{
    [Save("Traveller")]
    public struct Traveller : IComponentData
    {
        // Journey info
        public int2 destination;
        public TravelObjects useableVehicles;

        public void SetJourneyData(int2 destination, TravelObjects useableVehicles)
        {
            this.destination = destination;
            this.useableVehicles = useableVehicles;
        }
    }

    [Save("Path")]
    public struct PathElement : IBufferElementData
    {
        public float3 waypoint;
        public static implicit operator PathElement(float3 _waypoint)
        {
            return new PathElement() { waypoint = _waypoint };
        }
    }

    [Save("MovementInfo")]
    public struct MovementInfo : IComponentData
    {
        public int nextWaypointIndex; // Used by movement system
        public float3 nextWaypoint;
    }
}
namespace Tags
{
    [Save("Travelling")]
    public struct Travelling : IComponentData, IEnableableComponent { }

    [Save("WantsToTravel")]
    public struct WantsToTravel : IComponentData, IEnableableComponent { }
}