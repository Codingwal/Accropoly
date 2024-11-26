using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Components;
using Tags;

namespace Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(CreationSystemGroup))]
    public partial class ImmigrationSystem : SystemBase
    {
        private const float immigrationProbability = 0.1f; // 1 = 100%
        [BurstCompile]
        protected override void OnCreate()
        {
            RequireForUpdate<ConfigComponents.PrefabEntity>();
            RequireForUpdate<RunGame>();
        }
        [BurstCompile]
        protected override void OnUpdate()
        {
            Random rnd = new((uint)UnityEngine.Random.Range(1, 1000));
            float deltaTime = SystemAPI.Time.DeltaTime;
            var ecb = SystemAPI.GetSingleton<EndCreationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            Entity prefab = SystemAPI.GetSingleton<ConfigComponents.PrefabEntity>();

            NativeArray<Entity> homelessEntities = GetEntityQuery(typeof(Homeless)).ToEntityArray(Allocator.TempJob);
            NativeArray<Person> homelessPersonComponents = GetEntityQuery(typeof(Homeless), typeof(Person)).ToComponentDataArray<Person>(Allocator.TempJob);
            NativeArray<LocalTransform> homelessTransforms = GetEntityQuery(typeof(Homeless), typeof(LocalTransform)).ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            NativeArray<int> homelessIndex = new(1, Allocator.TempJob);
            homelessIndex[0] = 0;

            // Foreach active habitat with space
            Entities.WithAll<ActiveTile, HasSpace>().ForEach((Entity habitatEntity, ref Habitat habitat, in Tile habitatTile) =>
            {
                if (homelessIndex[0] < homelessEntities.Length) // If there is at least one homeless person left
                {
                    // Reimmigration

                    habitat.freeSpace--;
                    if (habitat.freeSpace == 0) ecb.RemoveComponent<HasSpace>(habitatEntity);

                    var homelessEntity = homelessEntities[homelessIndex[0]];

                    ecb.RemoveComponent<Homeless>(homelessEntity);

                    // Update homeTile
                    var personComponent = homelessPersonComponents[homelessIndex[0]];
                    personComponent.homeTile = habitatTile.pos;
                    ecb.SetComponent(homelessEntity, personComponent);

                    // Update position
                    float offset = (habitat.totalSpace - habitat.freeSpace - 2.5f) * 0.2f;
                    var homelessTransform = homelessTransforms[homelessIndex[0]];
                    homelessTransform.Position = new(2 * habitatTile.pos.x + offset, 0.5f, 2 * habitatTile.pos.y);
                    ecb.SetComponent(homelessEntity, homelessTransform);
                }
                else if (rnd.NextFloat() <= immigrationProbability * deltaTime) // Multiply with delta time bc immigrationProbability is per second, not per frame
                {
                    // Immigration

                    habitat.freeSpace--;
                    if (habitat.freeSpace == 0) ecb.RemoveComponent<HasSpace>(habitatEntity);

                    // Create new inhabitant for this house ("immigrant")
                    Entity entity = ecb.Instantiate(prefab);
                    ecb.AddComponent(entity, new NewPerson());
                    ecb.AddComponent(entity, new Person
                    {
                        homeTile = habitatTile.pos,
                        age = 0,
                    });
                    ecb.AddComponent(entity, new Worker { employer = new(-1) });
                    ecb.AddComponent<Traveller>(entity);
                    ecb.AddComponent<Travelling>(entity);
                    ecb.SetComponentEnabled<Travelling>(entity, false);

                    float offset = (habitat.totalSpace - habitat.freeSpace - 2.5f) * 0.2f;
                    float3 pos = new(2 * habitatTile.pos.x + offset, 0.5f, 2 * habitatTile.pos.y);
                    ecb.SetComponent(entity, LocalTransform.FromPositionRotationScale(pos, quaternion.identity, 0.1f));
                }
            }).WithDisposeOnCompletion(homelessEntities).WithDisposeOnCompletion(homelessPersonComponents).WithDisposeOnCompletion(homelessTransforms).WithDisposeOnCompletion(homelessIndex).Schedule();
        }
    }
}