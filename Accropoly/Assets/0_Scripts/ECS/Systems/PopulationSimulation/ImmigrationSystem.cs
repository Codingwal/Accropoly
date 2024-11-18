using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(CreationSystemGroup))]
public partial class ImmigrationSystem : SystemBase
{
    private const float immigrationProbability = 0.1f; // 1 = 100%
    [BurstCompile]
    protected override void OnCreate()
    {
        RequireForUpdate<ConfigComponents.PrefabEntity>();
        RequireForUpdate<RunGameTag>();
    }
    [BurstCompile]
    protected override void OnUpdate()
    {
        Random rnd = new((uint)UnityEngine.Random.Range(1, 1000));
        float deltaTime = SystemAPI.Time.DeltaTime;
        var ecb = SystemAPI.GetSingleton<EndCreationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        Entity prefab = SystemAPI.GetSingleton<ConfigComponents.PrefabEntity>();

        NativeArray<Entity> homelessEntities = GetEntityQuery(typeof(HomelessTag)).ToEntityArray(Allocator.TempJob);
        NativeArray<PersonComponent> homelessPersonComponents = GetEntityQuery(typeof(HomelessTag), typeof(PersonComponent)).ToComponentDataArray<PersonComponent>(Allocator.TempJob);
        NativeArray<LocalTransform> homelessTransforms = GetEntityQuery(typeof(HomelessTag), typeof(LocalTransform)).ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        NativeArray<int> homelessIndex = new(1, Allocator.TempJob);
        homelessIndex[0] = 0;

        // Foreach active habitat with space
        Entities.WithAll<ActiveTileTag, HasSpaceTag>().ForEach((Entity habitatEntity, ref Habitat habitat, in MapTileComponent habitatTile) =>
        {
            if (homelessIndex[0] < homelessEntities.Length) // If there is at least one homeless person left
            {
                // Reimmigration

                habitat.freeSpace--;
                if (habitat.freeSpace == 0) ecb.RemoveComponent<HasSpaceTag>(habitatEntity);

                var homelessEntity = homelessEntities[homelessIndex[0]];

                ecb.RemoveComponent<HomelessTag>(homelessEntity);

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
                if (habitat.freeSpace == 0) ecb.RemoveComponent<HasSpaceTag>(habitatEntity);

                // Create new inhabitant for this house ("immigrant")
                Entity entity = ecb.Instantiate(prefab);
                ecb.AddComponent(entity, new NewPersonTag());
                ecb.AddComponent(entity, new PersonComponent
                {
                    homeTile = habitatTile.pos,
                    age = 0,
                });
                ecb.AddComponent(entity, new Worker { employer = new(-1) });

                float offset = (habitat.totalSpace - habitat.freeSpace - 2.5f) * 0.2f;
                float3 pos = new(2 * habitatTile.pos.x + offset, 0.5f, 2 * habitatTile.pos.y);
                ecb.SetComponent(entity, LocalTransform.FromPositionRotationScale(pos, quaternion.identity, 0.1f));
            }
        }).WithDisposeOnCompletion(homelessEntities).WithDisposeOnCompletion(homelessPersonComponents).WithDisposeOnCompletion(homelessTransforms).WithDisposeOnCompletion(homelessIndex).Schedule();
    }
}
