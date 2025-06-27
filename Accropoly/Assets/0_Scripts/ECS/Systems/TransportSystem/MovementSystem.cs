using Components;
using Tags;
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
            RequireForUpdate<Traveller>();
            RequireForUpdate<RunGame>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            var buffer = SystemAPI.GetBuffer<EntityBufferElement>(SystemAPI.GetSingletonEntity<EntityGridHolder>());
            GameInfo gameInfo = SystemAPI.GetSingleton<GameInfo>();
            float deltaTime = gameInfo.deltaTime / 500;

            Entities.WithAll<Travelling>().ForEach((Entity entity, ref Traveller traveller, ref LocalTransform transform) =>
            {
                float3 nextPos = traveller.waypoints[traveller.nextWaypointIndex];
                Waypoint nextWaypoint;
                if (((int2)nextPos.xz / 2).Equals(traveller.destination))
                    nextWaypoint = new(nextPos, 1, false);
                else
                    nextWaypoint = WaypointSystem.waypoints[nextPos];

                float3 targetDir = nextPos - transform.Position;
                float t = 1 / (1 + math.distance(transform.Position, nextPos));
                float targetVel = math.lerp(math.length(traveller.velocity), nextWaypoint.velocity, t);

                float3 acceleration = (targetVel * targetDir) - traveller.velocity;

                if (math.length(acceleration) > traveller.maxAcceleration)
                {
                    Debug.Log("!");
                    acceleration = math.normalize(acceleration) * traveller.maxAcceleration;
                }

                // Debug.Log($"old: v={traveller.velocity}, p={transform.Position}");

                traveller.velocity += acceleration * deltaTime;
                transform.Position += traveller.velocity * deltaTime;

                // Debug.Log($"nP={nextPos}, tD={targetDir}, t={t}, tV={targetVel}, a={acceleration}, v={traveller.velocity}, p={transform.Position}");

                if (math.distancesq(transform.Position, nextPos) < 0.1f)
                {
                    traveller.nextWaypointIndex++;
                    if (traveller.nextWaypointIndex == traveller.waypoints.Length) // Reached destination
                    {
                        Debug.Log("Reached destination");
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