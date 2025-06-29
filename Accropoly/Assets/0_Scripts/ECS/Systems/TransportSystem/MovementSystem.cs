using System;
using Components;
using Tags;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
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
        private static NativeList<RaycastData> raycasts;
        protected override void OnCreate()
        {
            var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<Travelling>();
            RequireForUpdate(GetEntityQuery(builder));
            builder.Dispose();

            RequireForUpdate<EntityGridHolder>();

            raycasts = new(Allocator.Persistent);
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            var buffer = SystemAPI.GetBuffer<EntityBufferElement>(SystemAPI.GetSingletonEntity<EntityGridHolder>());
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            GameInfo gameInfo = SystemAPI.GetSingleton<GameInfo>();
            float deltaTime = gameInfo.deltaTime / gameSecondsPerMovementSecond;

            raycasts.Clear();

            // Initialize person colliders (so that they are hit by the rays)
            CollisionFilter colliderFilter = new()
            {
                BelongsTo = (uint)CollisionLayers.Cars,
                CollidesWith = (uint)CollisionLayers.CarRays,
            };
            if (SystemAPI.HasSingleton<LoadGame>())
            {
                Entities.WithAll<Person>().ForEach((ref PhysicsCollider collider) =>
                {
                    collider.Value.Value.SetCollisionFilter(colliderFilter);
                }).Run();
            }
            else
            {
                Entities.WithAll<NewPerson>().ForEach((ref PhysicsCollider collider) =>
                {
                    collider.Value.Value.SetCollisionFilter(colliderFilter);
                }).Run();
            }

            if (!SystemAPI.HasSingleton<RunGame>())
                return;
            // Move cars
            Entities.WithAll<Travelling>().ForEach((Entity entity, ref Traveller traveller, ref LocalTransform transform) =>
            {
                // Instantly teleport to first waypoint
                if (traveller.nextWaypointIndex == 1)
                {
                    transform.Position = traveller.waypoints[1];
                    traveller.nextWaypointIndex = 2;

                    // Register at waypoint
                    float3 nextPosTmp = traveller.waypoints[traveller.nextWaypointIndex];
                    Waypoint tmp = WaypointSystem.waypoints[nextPosTmp];
                    tmp.registeredObjects++;
                    WaypointSystem.waypoints[nextPosTmp] = tmp;
                }

                // Get data related to the next waypoint
                float3 nextPos = traveller.waypoints[traveller.nextWaypointIndex];
                Waypoint nextWaypoint = WaypointSystem.waypoints[nextPos];
                float nextVelocity = nextWaypoint.stop ? 0 : nextWaypoint.velocity;

                float3 targetDirection = math.normalize(nextPos - transform.Position);
                float targetSpeed = math.lerp(math.length(traveller.velocity), nextVelocity, 1 / (1 + math.distance(transform.Position, nextPos)));

                // Prevent collisions with other cars directly in front of this one
                var raycastData = CastRay(transform, physicsWorld);
                if (raycastData.hit)
                {
                    targetSpeed = 0;
                    Debug.Log("Hit, waiting!");
                }

                float3 acceleration = (targetSpeed * targetDirection) - traveller.velocity;

                // Clamp acceleration
                if (math.lengthsq(acceleration) > math.square(traveller.maxAcceleration))
                    acceleration = math.normalize(acceleration) * traveller.maxAcceleration;

                // Update velocity and position
                traveller.velocity += acceleration;
                transform.Position += traveller.velocity * deltaTime;

                // Update rotation
                if (!traveller.velocity.xz.Equals(0))
                {
                    float3 v = traveller.velocity;
                    float rotY = math.atan(v.x / (v.z + 0.00001f));
                    transform.Rotation = quaternion.EulerXYZ(0, rotY, 0);
                }

                if (math.distancesq(transform.Position, nextPos) < math.square(0.1f)) // Reached waypoint
                {
                    // de-register from now reached waypoint
                    nextWaypoint.registeredObjects--;
                    WaypointSystem.waypoints[nextPos] = nextWaypoint;

                    traveller.nextWaypointIndex++; // Update targeted waypoint

                    if (traveller.nextWaypointIndex == traveller.waypoints.Length - 1) // Reached last waypoint
                    {
                        transform.Position.xz = traveller.destination * 2; // Teleport to destination
                        ecb.SetComponentEnabled<Travelling>(entity, false);
                    }
                    else
                    {
                        // Register at new next waypoint
                        nextPos = traveller.waypoints[traveller.nextWaypointIndex];
                        Waypoint tmp = WaypointSystem.waypoints[nextPos];
                        tmp.registeredObjects++;
                        WaypointSystem.waypoints[nextPos] = tmp;
                    }
                }
            }).WithName("movementJob").Schedule();
        }
        protected override void OnDestroy()
        {
            raycasts.Dispose();
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

            foreach (RaycastData data in raycasts)
            {
                if (data.hit)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(data.raycastInput.Start, data.closestHit.Position);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(data.closestHit.Position, data.raycastInput.End);
                }
                else
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(data.raycastInput.Start, data.raycastInput.End);
                }
            }
        }

        private static RaycastData CastRay(LocalTransform transform, in CollisionWorld collisionWorld)
        {
            try
            {
                RaycastInput raycastInput = new()
                {
                    Start = transform.Position + 0.15f * transform.Forward(),
                    End = transform.Position + 1 * transform.Forward(),
                    Filter = new()
                    {
                        BelongsTo = (uint)CollisionLayers.CarRays,
                        CollidesWith = (uint)CollisionLayers.Cars,
                    }
                };

                bool hit = collisionWorld.CastRay(raycastInput, out var closestHit);

                RaycastData data = new(hit, raycastInput, closestHit);

                raycasts.Add(data);

                return data;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError($"{transform.Position}, {transform.Forward()}, {transform.Rotation}");
                return default;
            }
        }

        private struct RaycastData
        {
            public bool hit;
            public RaycastInput raycastInput;
            public Unity.Physics.RaycastHit closestHit;
            public RaycastData(bool hit, RaycastInput raycastInput, Unity.Physics.RaycastHit closestHit)
            {
                this.hit = hit;
                this.raycastInput = raycastInput;
                this.closestHit = closestHit;
            }
        }
    }

    public enum CollisionLayers : uint
    {
        Cars = 1 << 10,
        CarRays = 1 << 11,
    }
}