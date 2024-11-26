using Components;
using Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    public partial class MovementSystem : SystemBase
    {
        private const float speed = 3f;
        private const float waypointRange = 0.1f;
        private const float waypointRangeSqr = waypointRange * waypointRange;
        protected override void OnCreate()
        {
            RequireForUpdate<Traveller>();
            RequireForUpdate<RunGame>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            float deltaTime = SystemAPI.Time.DeltaTime;

            Entities.WithAll<Travelling>().ForEach((Entity entity, ref Traveller traveller, ref LocalTransform transform) =>
            {
                Waypoint nextWaypoint = traveller.waypoints[traveller.nextWaypointIndex];

                if (math.distancesq(transform.Position.xz, nextWaypoint.pos) <= waypointRangeSqr)
                {
                    traveller.nextWaypointIndex++;
                    Debug.Log(nextWaypoint);

                    if (traveller.nextWaypointIndex == traveller.waypoints.Length) // If the destination has been reached
                    {
                        ecb.SetComponentEnabled<Travelling>(entity, false);
                        return;
                    }

                    nextWaypoint = traveller.waypoints[traveller.nextWaypointIndex];
                }

                float2 direction = nextWaypoint.pos -= transform.Position.xz;
                float2 directionNormalized = math.normalize(direction);
                transform.Position.xz += directionNormalized * speed * deltaTime;
            }).WithBurst(Unity.Burst.FloatMode.Fast, Unity.Burst.FloatPrecision.Low).Schedule();
        }
    }
}