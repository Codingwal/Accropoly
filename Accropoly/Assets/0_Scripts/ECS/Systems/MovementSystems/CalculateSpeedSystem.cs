using Components;
using Components.WaypointComponents;
using Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Systems
{
    [UpdateBefore(typeof(FollowCurveSystem))]
    [UpdateAfter(typeof(TravelProgressSystem))]
    public partial struct CalculateSpeedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WaypointsData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new CalculateSpeedJob()
            {
                waypointLookup = SystemAPI.GetComponentLookup<Waypoint>(isReadOnly: true),
                waypointsData = SystemAPI.GetSingleton<WaypointsData>()
            }.Schedule();
        }

        [BurstCompile]
        [WithAll(typeof(Travelling))]
        public partial struct CalculateSpeedJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<Waypoint> waypointLookup;
            public WaypointsData waypointsData;
            public void Execute(ref Speed speed, in MovementInfo movementInfo, in CurveFollower curveFollower)
            {
                Entity nextWaypointEntity = waypointsData.waypoints[movementInfo.nextWaypoint];
                float nextWaypointSpeed = waypointLookup[nextWaypointEntity].velocity;

                speed.value = math.lerp(math.max(speed.value, 1), nextWaypointSpeed, curveFollower.timeAlongCurve);
            }
        }
    }
}