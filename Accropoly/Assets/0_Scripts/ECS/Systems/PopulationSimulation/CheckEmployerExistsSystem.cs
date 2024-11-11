using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

public partial class CheckEmployerExistsSystem : SystemBase
{
    private EntityQuery newTilesQuery;
    protected override void OnCreate()
    {
        RequireForUpdate<Worker>();
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

        // Iterate over all people. Make them homeless if their home is one of the deleted tiles
        Dependency = Entities.WithNone<UnemployedTag>().ForEach((Entity entity, ref Worker worker) =>
        {
            int2 employerPos = worker.employer;
            if (newTilePositions.Contains(employerPos))
            {
                worker.employer = new(-1);
                ecb.AddComponent<UnemployedTag>(entity);
            }
        }).WithoutBurst().WithDisposeOnCompletion(newTilePositions).Schedule(Dependency);
    }
}