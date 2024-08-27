using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct TileSpawnerSystem : ISystem
{

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TileSpawnerConfig>();
        state.RequireForUpdate<WorldData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        TileSpawnerConfig config = SystemAPI.GetSingleton<TileSpawnerConfig>();
        WorldData worldData = SystemAPI.GetSingleton<WorldData>();
        ref MapData mapData = ref worldData.map;

        NativeArray<Entity> tiles = entityManager.Instantiate(config.tilePrefab, mapData.TotalSize, Allocator.Temp);

        for (int x = 0; x < mapData.size.x; x++)
        {
            for (int y = 0; y < mapData.size.y; y++)
            {
                Entity entity = tiles[mapData.GetIndex(x, y)];

                entityManager.SetComponentData(entity, LocalTransform.FromPosition(2 * x, 1, 2 * y));
            }
        }
        tiles.Dispose();
    }
}