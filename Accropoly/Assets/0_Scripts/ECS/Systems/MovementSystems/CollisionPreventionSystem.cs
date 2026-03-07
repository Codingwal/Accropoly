using Components;
using Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    [UpdateBefore(typeof(FollowCurveSystem))]
    [UpdateAfter(typeof(ObeyJunctionsSystem))]
    [UpdateAfter(typeof(CalculateSpeedSystem))]
    public partial struct CollisionPreventionSystem : ISystem
    {
        private NativeList<RaycastData> raycasts;

        public void OnCreate(ref SystemState state)
        {
            raycasts = new(Allocator.Persistent);

            foreach (var collider in SystemAPI.Query<RefRW<PhysicsCollider>>().WithAll<Person>())
            {
                collider.ValueRW.Value.Value.SetCollisionFilter(new CollisionFilter()
                {
                    BelongsTo = (uint)CollisionLayers.Cars,
                    CollidesWith = (uint)CollisionLayers.CarRays
                });
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            raycasts.Clear();

            foreach (var collider in SystemAPI.Query<RefRW<PhysicsCollider>>().WithAll<NewPerson>())
            {
                collider.ValueRW.Value.Value.SetCollisionFilter(new CollisionFilter()
                {
                    BelongsTo = (uint)CollisionLayers.Cars,
                    CollidesWith = (uint)CollisionLayers.CarRays
                });
            }

            new PreventCollisionsJob()
            {
                collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld,
                raycastsInfo = raycasts, // Passing by value is ok because NativeList is basically a reference to UnsafeList
            }.Schedule();
        }

        public void OnDestroy(ref SystemState state)
        {
            raycasts.Dispose();
        }

        public void DrawGizmos()
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

        [BurstCompile]
        [WithAll(typeof(Travelling))]
        public partial struct PreventCollisionsJob : IJobEntity
        {
            public NativeList<RaycastData> raycastsInfo; // For debugging
            public CollisionWorld collisionWorld;

            public void Execute(ref Speed speed, in LocalTransform transform)
            {
                // Prevent collisions with other cars directly in front of this one
                var raycastData = CastRay(transform);
                if (raycastData.hit)
                    speed.value = 0;
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
        }
    }

    public enum CollisionLayers : uint
    {
        Cars = 1 << 10,
        CarRays = 1 << 11,
    }
}