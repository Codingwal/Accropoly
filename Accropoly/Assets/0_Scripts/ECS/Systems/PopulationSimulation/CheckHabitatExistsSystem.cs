using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial class CheckHabitatExistsSystem : SystemBase
{
    private EntityQuery newTilesQuery;
    protected override void OnCreate()
    {
        RequireForUpdate<PersonComponent>();
        newTilesQuery = GetEntityQuery(typeof(NewTileTag));
    }
    protected override void OnUpdate()
    {
        int newTilesCount = newTilesQuery.CalculateEntityCount();

        if (newTilesCount == 0) return;

        // Get all positions of deleted (replaced) tiles
        NativeArray<int2> newTilePositions = new(newTilesCount, Allocator.TempJob);
        int index = 0;
        Entities.WithAll<NewTileTag>().ForEach((in MapTileComponent mapTileComponent) =>
        {
            newTilePositions[index] = mapTileComponent.pos;
            index++;
        }).Run();

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        Unity.Mathematics.Random rnd = new((uint)UnityEngine.Random.Range(1, 1000));

        // Iterate over all people. Make them homeless if their home is one of the deleted tiles
        Dependency = Entities.WithNone<HomelessTag>().ForEach((Entity entity, ref PersonComponent person) =>
        {
            int2 homeTilePos = person.homeTile;
            if (newTilePositions.Contains(homeTilePos))
            {
                person.homeTile = new(-1);
                ecb.AddComponent<HomelessTag>(entity);

                // Homeless people are collected at a special position
                LocalTransform transform = SystemAPI.GetComponent<LocalTransform>(entity);
                transform.Position = new(-1 + rnd.NextFloat(-0.5f, 0.5f), 0.5f, -1 + rnd.NextFloat(-0.5f, 0.5f));
                ecb.SetComponent(entity, transform);
            }
        }).WithoutBurst().WithDisposeOnCompletion(newTilePositions).Schedule(Dependency);
    }
}