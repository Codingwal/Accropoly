using Components;
using Tags;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct ValidatePathSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WaypointsData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new ValidatePathJob()
        {
            waypointsData = SystemAPI.GetSingleton<WaypointsData>(),
            travellingLookup = SystemAPI.GetComponentLookup<Travelling>(),
            wantsToTravelLookup = SystemAPI.GetComponentLookup<WantsToTravel>(),
        }.Schedule();
    }
    
    [BurstCompile]
    [WithAll(typeof(Travelling))]
    public partial struct ValidatePathJob : IJobEntity
    {
        public WaypointsData waypointsData;
        public ComponentLookup<Travelling> travellingLookup;
        public ComponentLookup<WantsToTravel> wantsToTravelLookup;
        public void Execute(Entity entity, ref LocalTransform transform, ref MovementInfo movementInfo, in DynamicBuffer<PathElement> path)
        {
            // Check if destination still exists and stop otherwise
            if (!WaypointExists(path[^1].waypoint))
            {
                Debug.LogWarning("Destination does not exist! Stopping.");
                travellingLookup.SetComponentEnabled(entity, false);
                return;
            }

            // Check if path is valid
            for (int i = movementInfo.nextWaypointIndex; i < path.Length; i++)
            {
                float3 waypoint = path[i].waypoint;
                if (WaypointExists(waypoint)) continue;

                // If there is an invalid waypoint...

                // Find next valid waypoint
                while (!WaypointExists(path[movementInfo.nextWaypointIndex].waypoint))
                    movementInfo.nextWaypointIndex++;

                // TODO: Remake
                // Teleport to next waypoint and request a new path
                transform.Position = path[movementInfo.nextWaypointIndex].waypoint;
                travellingLookup.SetComponentEnabled(entity, false);
                wantsToTravelLookup.SetComponentEnabled(entity, true);
                return;
            }
        }
        private bool WaypointExists(float3 pos)
        {
            return waypointsData.waypoints.ContainsKey(pos);
        }
    }
}