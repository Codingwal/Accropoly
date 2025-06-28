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
            float deltaTime = gameInfo.deltaTime / 1000;

            Entities.WithAll<Travelling>().ForEach((Entity entity, ref Traveller traveller, ref LocalTransform transform) =>
            {
                float3 nextPos = traveller.waypoints[traveller.nextWaypointIndex];
                float nextVelocity;

                if (((int2)nextPos.xz / 2).Equals(traveller.destination))
                    nextVelocity = 1;
                else
                    nextVelocity = WaypointSystem.waypoints[nextPos].velocity;

                float3 targetDirection = math.normalize(nextPos - transform.Position);
                float targetSpeed = math.lerp(math.length(traveller.velocity), nextVelocity, 1 / (1 + math.distance(transform.Position, nextPos)));

                float3 acceleration = (targetSpeed * targetDirection) - traveller.velocity;
                acceleration /= deltaTime;

                if (math.lengthsq(acceleration) > math.square(traveller.maxAcceleration))
                {
                    Debug.Log("Clamping acceleration");
                    acceleration = math.normalize(acceleration) * traveller.maxAcceleration;
                }

                traveller.velocity += acceleration * deltaTime;
                transform.Position += traveller.velocity * deltaTime;

                if (math.distancesq(transform.Position, nextPos) < math.square(0.1f))
                {
                    traveller.nextWaypointIndex++;
                    if (traveller.nextWaypointIndex == traveller.waypoints.Length) // Reached destination
                    {
                        transform.Position.xz = traveller.destination * 2;
                        ecb.SetComponentEnabled<Travelling>(entity, false);
                    }
                }
            }).Schedule();
        }

        public void DrawGizmos()
        {
            Gizmos.color = Color.green;

            Entities.WithAll<Travelling>().ForEach((in Traveller traveller) =>
            {
                for (int i = 1; i < traveller.waypoints.Length; i++)
                {
                    Gizmos.DrawLine(traveller.waypoints[i - 1], traveller.waypoints[i]);
                }
            }).Run();
        }
    }
}