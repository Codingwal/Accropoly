using Components;
using Tags;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    /// <summary>
    /// Move people that are currently travelling (Travelling tag)
    /// Does not calculate the path, only moves the person along the waypoints managed by PathfindingSystem
    /// </summary>
    public partial class MovementSystem : SystemBase
    {
        public const float gameSecondsPerMovementSecond = 8000;
        protected override void OnCreate()
        {
            var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<Travelling>();
            RequireForUpdate(GetEntityQuery(builder));
            builder.Dispose();

            RequireForUpdate<RunGame>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            var buffer = SystemAPI.GetBuffer<EntityBufferElement>(SystemAPI.GetSingletonEntity<EntityGridHolder>());
            GameInfo gameInfo = SystemAPI.GetSingleton<GameInfo>();
            float deltaTime = gameInfo.deltaTime / gameSecondsPerMovementSecond;

            Entities.WithAll<Travelling>().ForEach((Entity entity, ref Traveller traveller, ref LocalTransform transform) =>
            {
                // Instantly teleport to first waypoint
                if (traveller.nextWaypointIndex == 1)
                {
                    transform.Position = traveller.waypoints[1];
                    traveller.nextWaypointIndex = 2;

                    float3 nextPosTmp = traveller.waypoints[traveller.nextWaypointIndex];
                    Waypoint tmp = WaypointSystem.waypoints[nextPosTmp];
                    tmp.registeredObjects++;
                    WaypointSystem.waypoints[nextPosTmp] = tmp;
                }

                float3 nextPos = traveller.waypoints[traveller.nextWaypointIndex];
                Waypoint nextWaypoint = WaypointSystem.waypoints[nextPos];
                float nextVelocity = nextWaypoint.stop ? 0 : nextWaypoint.velocity;

                float3 targetDirection = math.normalize(nextPos - transform.Position);
                float targetSpeed = math.lerp(math.length(traveller.velocity), nextVelocity, 1 / (1 + math.distance(transform.Position, nextPos)));

                float3 acceleration = (targetSpeed * targetDirection) - traveller.velocity;

                if (math.lengthsq(acceleration) > math.square(traveller.maxAcceleration))
                    acceleration = math.normalize(acceleration) * traveller.maxAcceleration;

                traveller.velocity += acceleration;
                transform.Position += traveller.velocity * deltaTime;

                if (math.distancesq(transform.Position, nextPos) < math.square(0.1f))
                {
                    nextWaypoint.registeredObjects--;
                    WaypointSystem.waypoints[nextPos] = nextWaypoint;

                    traveller.nextWaypointIndex++;
                    if (traveller.nextWaypointIndex == traveller.waypoints.Length - 1) // Reached last waypoint
                    {
                        transform.Position.xz = traveller.destination * 2; // Teleport to destination
                        ecb.SetComponentEnabled<Travelling>(entity, false);
                    }
                    else
                    {
                        nextPos = traveller.waypoints[traveller.nextWaypointIndex];
                        Waypoint tmp = WaypointSystem.waypoints[nextPos];
                        tmp.registeredObjects++;
                        WaypointSystem.waypoints[nextPos] = tmp;
                    }
                }
            }).Schedule();
        }

        public void DrawGizmos()
        {
            Gizmos.color = Color.green;

            Entities.WithAll<Travelling>().ForEach((in Traveller traveller) =>
            {
                for (int i = traveller.nextWaypointIndex; i < traveller.waypoints.Length; i++)
                {
                    Gizmos.DrawLine(traveller.waypoints[i - 1], traveller.waypoints[i]);
                }
            }).Run();
        }
    }
}