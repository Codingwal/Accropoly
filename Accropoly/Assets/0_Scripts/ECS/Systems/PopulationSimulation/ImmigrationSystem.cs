using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Components;
using Tags;
using UnityEngine;

namespace Systems
{
    /// <summary>
    /// Reimmigrate homeless people
    /// If there is space left, create a new person with a specific probability
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(CreationSystemGroup))]
    public partial class ImmigrationSystem : SystemBase
    {
        private const float immigrationProbability = 0.1f; // 1 = 100%
        [BurstCompile]
        protected override void OnUpdate()
        {
            Unity.Mathematics.Random rnd = new((uint)UnityEngine.Random.Range(1, 1000));
            float deltaTime = SystemAPI.Time.DeltaTime;
            var ecb = SystemAPI.GetSingleton<EndCreationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            NativeArray<Entity> homelessEntities = GetEntityQuery(typeof(Homeless)).ToEntityArray(Allocator.TempJob);
            NativeArray<Person> homelessPersonComponents = GetEntityQuery(typeof(Homeless), typeof(Person)).ToComponentDataArray<Person>(Allocator.TempJob);
            NativeReference<int> homelessIndex = new(0, Allocator.TempJob);

            new ImmigrationJob
            {
                ecb = ecb,
                deltaTime = deltaTime,
                rnd = rnd,
                homelessEntities = homelessEntities,
                homelessPersonComponents = homelessPersonComponents,
                homelessIndex = homelessIndex,
            }.Schedule();

            homelessEntities.Dispose(Dependency);
            homelessPersonComponents.Dispose(Dependency);
            homelessIndex.Dispose(Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(ActiveTile), typeof(HasSpace))]
        private partial struct ImmigrationJob : IJobEntity
        {
            public EntityCommandBuffer ecb;
            public float deltaTime;
            public Unity.Mathematics.Random rnd;
            public NativeArray<Entity> homelessEntities;
            public NativeArray<Person> homelessPersonComponents;
            public NativeReference<int> homelessIndex;
            public void Execute(Entity habitatEntity, ref Habitat habitat, in Tile habitatTile)
            {
                if (homelessIndex.Value < homelessEntities.Length) // If there is at least one homeless person left
                {
                    // Reimmigration

                    habitat.freeSpace--;
                    if (habitat.freeSpace == 0) ecb.RemoveComponent<HasSpace>(habitatEntity);

                    var homelessEntity = homelessEntities[homelessIndex.Value];

                    ecb.RemoveComponent<Homeless>(homelessEntity);

                    // Update homeTile
                    var personComponent = homelessPersonComponents[homelessIndex.Value];
                    personComponent.homeTile = habitatTile.pos;
                    ecb.SetComponent(homelessEntity, personComponent);

                    // Update position
                    float3 pos = new(2 * habitatTile.pos.x, 0.8f, 2 * habitatTile.pos.y);
                    ecb.SetComponent(homelessEntity, LocalTransform.FromPositionRotationScale(pos, quaternion.identity, 0.1f));

                    homelessIndex.Value++; // ? 
                }
                else if (rnd.NextFloat() <= immigrationProbability * deltaTime) // Multiply with delta time bc immigrationProbability is per second, not per frame
                {
                    // Immigration

                    habitat.freeSpace--;
                    if (habitat.freeSpace == 0) ecb.RemoveComponent<HasSpace>(habitatEntity);

                    // Create new inhabitant for this house ("immigrant")
                    Entity entity = ecb.CreateEntity();

                    // Rendering components will be added automatically by PopulationSetupSystem

                    ecb.AddComponent(entity, new NewPerson());
                    ecb.AddComponent(entity, new Person
                    {
                        homeTile = habitatTile.pos,
                        age = 0,
                    });
                    ecb.AddComponent(entity, new Worker { employer = new(-1) });

                    ecb.AddComponent<WantsToTravel>(entity);
                    ecb.SetComponentEnabled<WantsToTravel>(entity, false);

                    ecb.AddComponent<Traveller>(entity);
                    ecb.AddComponent<MovementInfo>(entity);
                    ecb.AddBuffer<PathElement>(entity);
                    ecb.AddComponent<Speed>(entity);
                    ecb.AddComponent<CurveFollower>(entity);

                    ecb.AddComponent<Travelling>(entity);
                    ecb.SetComponentEnabled<Travelling>(entity, false);

                    ecb.AddComponent<FreeTime>(entity);

                    float3 pos = new(2 * habitatTile.pos.x, 0.8f, 2 * habitatTile.pos.y);
                    ecb.AddComponent(entity, LocalTransform.FromPositionRotationScale(pos, quaternion.identity, 0.1f));
                }
            }
        }
    }
}