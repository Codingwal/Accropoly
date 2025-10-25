using Components;
using Components.WaypointComponents;
using Tags;
using Unity.Burst;
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
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class MovementSystem : SystemBase
    {
        public const float gameSecondsPerMovementSecond = 8000;
        private NativeList<RaycastData> raycasts;
        EntityQuery travellingObjects;
        protected override void OnCreate()
        {
            travellingObjects = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Travelling, Traveller, LocalTransform>()
                .Build(this);

            RequireForUpdate(travellingObjects);

            raycasts = new(Allocator.Persistent);
        }
        protected override void OnUpdate()
        {
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

            WaypointsData waypointsData = SystemAPI.GetSingleton<WaypointsData>();

            // Wait for WaypointSystem to setup waypoints
            if (waypointsData.waypoints.IsEmpty)
                return;

            new MoveObjectsJob()
            {
                ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged),
                waypointsData = waypointsData,
                collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld,
                deltaTime = SystemAPI.GetSingleton<GameInfo>().fixedDeltaTime / gameSecondsPerMovementSecond,
                waypointLookup = SystemAPI.GetComponentLookup<Waypoint>(),
                transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(),
                junctionLookup = SystemAPI.GetComponentLookup<Junction>(),
                raycastsInfo = raycasts, // Passing by value is ok because NativeList is basically a reference to UnsafeList
            }.Schedule(travellingObjects);
        }
        protected override void OnDestroy()
        {
            raycasts.Dispose();
        }

        public void DrawGizmos(bool debugPath, bool debugRaycasts, bool debugCurrentTarget)
        {
            if (!SystemAPI.HasSingleton<RunGame>())
                return;

            if (debugPath)
            {
                Gizmos.color = Color.green;
                foreach (RefRO<Traveller> traveller in SystemAPI.Query<RefRO<Traveller>>().WithAll<Travelling>())
                {
                    for (int i = traveller.ValueRO.nextWaypointIndex + 1; i < traveller.ValueRO.waypoints.Length; i++)
                    {
                        float3 from = traveller.ValueRO.waypoints[i - 1];
                        float3 to = traveller.ValueRO.waypoints[i];
                        Gizmos.DrawLine(from, to);
                    }

                }
            }

            if (debugCurrentTarget)
            {
                Gizmos.color = Color.yellow;
                foreach (var (traveller, transform) in SystemAPI.Query<RefRO<Traveller>, RefRO<LocalTransform>>().WithAll<Travelling>())
                {
                    float3 from = transform.ValueRO.Position;
                    float3 to = traveller.ValueRO.NextWaypoint;
                    Gizmos.DrawLine(from, to);
                }
            }

            if (debugRaycasts)
            {
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
        }

        public struct RaycastData
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

        /// <summary>
        /// Move all objects such as cars along the waypoints
        /// </summary>
        /// <remarks>Make sure the object has the travelling tag</remarks>
        [BurstCompile]
        private partial struct MoveObjectsJob : IJobEntity
        {
            public EntityCommandBuffer ecb;
            public WaypointsData waypointsData;
            public CollisionWorld collisionWorld;
            public float deltaTime;
            public ComponentLookup<Waypoint> waypointLookup;
            public ComponentLookup<LocalTransform> transformLookup;
            public ComponentLookup<Junction> junctionLookup;
            public NativeList<RaycastData> raycastsInfo; // For debugging
            public void Execute(Entity entity, ref Traveller traveller)
            {
                ref LocalTransform transform = ref transformLookup.GetRefRW(entity).ValueRW;

                // Start with second waypoint as target (object starts at first waypoint)
                if (traveller.nextWaypointIndex == 0)
                {
                    traveller.nextWaypointIndex++;
                    RegisterAtWaypoint(GetEntity(traveller.NextWaypoint));
                }

                if (!WaypointExists(traveller.NextWaypoint))
                {
                    while (!WaypointExists(traveller.NextWaypoint))
                    {
                        traveller.nextWaypointIndex++;

                        // Return (don't move) if even the destination waypoint does not exist
                        if (traveller.nextWaypointIndex == traveller.waypoints.Length)
                            return;
                    }
                    RegisterAtWaypoint(GetEntity(traveller.NextWaypoint));
                }

                // Get data related to the next waypoint
                Entity nextWaypoint = GetEntity(traveller.NextWaypoint);
                float nextVelocity = waypointLookup.GetRefRO(nextWaypoint).ValueRO.velocity;
                float3 nextPos = transformLookup.GetRefRO(nextWaypoint).ValueRO.Position;

                // Calculate ideal velocity (won't be reached as acceleration is clamped)
                float3 targetDirection = math.normalize(nextPos - transform.Position);
                float targetSpeed = math.lerp(math.length(traveller.velocity), nextVelocity, 1 / (1 + math.distance(transform.Position, nextPos))); // Slowly reach target speed

                // Stop at red lights / give way
                if (junctionLookup.HasComponent(nextWaypoint))
                {
                    var junctionData = junctionLookup.GetRefRW(nextWaypoint);
                    if (junctionData.ValueRO.stop)
                        targetSpeed = 0;
                }

                // Prevent collisions with other cars directly in front of this one
                var raycastData = CastRay(transform);
                if (raycastData.hit)
                    targetSpeed = 0;

                // Calculate ideal acceleration
                float3 acceleration = (targetSpeed * targetDirection) - traveller.velocity;

                // Clamp acceleration
                if (math.lengthsq(acceleration) > math.square(traveller.maxAcceleration))
                    acceleration = math.normalize(acceleration) * traveller.maxAcceleration;

                HandlePhysics(ref traveller, ref transform, acceleration);

                CheckIfWaypointReached(entity, ref traveller, ref transform, nextWaypoint, nextPos);
            }
            private Entity GetEntity(float3 pos)
            {
                return waypointsData.waypoints[pos];
            }
            private bool WaypointExists(float3 pos)
            {
                return waypointsData.waypoints.ContainsKey(pos);
            }
            private RaycastData CastRay(LocalTransform transform)
            {
                RaycastInput raycastInput = new()
                {
                    Start = transform.Position + 0.15f * transform.Forward(),
                    End = transform.Position + 0.6f * transform.Forward(),
                    Filter = new()
                    {
                        BelongsTo = (uint)CollisionLayers.CarRays,
                        CollidesWith = (uint)CollisionLayers.Cars,
                    }
                };
                bool hit = collisionWorld.CastRay(raycastInput, out var closestHit);
                RaycastData data = new(hit, raycastInput, closestHit);
                raycastsInfo.Add(data);
                return data;
            }
            private void HandlePhysics(ref Traveller traveller, ref LocalTransform transform, float3 acceleration) // Update velocity, position and rotation
            {
                // Update velocity and position
                traveller.velocity += acceleration;
                transform.Position += traveller.velocity * deltaTime;

                // Update rotation
                if (math.lengthsq(traveller.velocity.xz) > math.square(0.1)) // Only update if the car is moving (preserve current state otherwise)
                {
                    float rotY = math.atan2(traveller.velocity.x, traveller.velocity.z + 0.00001f);
                    transform.Rotation = quaternion.EulerXYZ(0, rotY, 0);
                }
            }
            private void CheckIfWaypointReached(Entity entity, ref Traveller traveller, ref LocalTransform transform, Entity waypoint, float3 waypointPos)
            {
                if (math.distancesq(transform.Position, waypointPos) < math.square(0.3f)) // Reached waypoint
                {
                    DeregisterAtWaypoint(waypoint);

                    traveller.nextWaypointIndex++; // Update targeted waypoint

                    if (traveller.nextWaypointIndex == traveller.waypoints.Length) // Reached last waypoint
                    {
                        transform.Position = traveller.waypoints[^1]; // Teleport to destination
                        ecb.SetComponentEnabled<Travelling>(entity, false);
                    }
                    else
                    {
                        // If the waypoint does not exist, the next iteration will handle choosing a new one and registering there
                        if (WaypointExists(traveller.NextWaypoint))
                            RegisterAtWaypoint(GetEntity(traveller.NextWaypoint));
                    }
                }
            }
            private void RegisterAtWaypoint(Entity waypoint)
            {
                if (junctionLookup.HasComponent(waypoint))
                    junctionLookup.GetRefRW(waypoint).ValueRW.registeredObjects++;
            }
            private void DeregisterAtWaypoint(Entity waypoint)
            {
                if (junctionLookup.HasComponent(waypoint))
                    junctionLookup.GetRefRW(waypoint).ValueRW.registeredObjects--;
            }
        }
    }

    public enum CollisionLayers : uint
    {
        Cars = 1 << 10,
        CarRays = 1 << 11,
    }
}