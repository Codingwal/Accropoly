using Components;
using Components.WaypointComponents;
using Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    [UpdateBefore(typeof(FollowCurveSystem))]
    [UpdateAfter(typeof(CalculateSpeedSystem))]
    public partial struct ObeyJunctionsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WaypointsData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new ObeyJunctionsJob()
            {
                junctionLookup = SystemAPI.GetComponentLookup<Junction>(isReadOnly: true),
                waypointsData = SystemAPI.GetSingleton<WaypointsData>()
            }.Schedule();
        }

        [BurstCompile]
        [WithAll(typeof(Travelling))]
        private partial struct ObeyJunctionsJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<Junction> junctionLookup;
            public WaypointsData waypointsData;
            public void Execute(ref Speed speed, in MovementInfo movementInfo, in LocalTransform transform)
            {
                Entity nextWaypointEntity = waypointsData.waypoints[movementInfo.nextWaypoint];

                if (junctionLookup.TryGetComponent(nextWaypointEntity, out var junctionData))
                {
                    if (!junctionData.stop)
                        return;

                    // smoothly slow down
                    float distSqr = math.distancesq(transform.Position, movementInfo.nextWaypoint);
                    speed.value = math.clamp(distSqr, 0, 1) * speed.value;
                }
            }
        }
    }
}