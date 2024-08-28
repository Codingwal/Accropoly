using System;
using System.ComponentModel;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct TileSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TileSpawnerConfig>();
        state.RequireForUpdate<RunGameTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        EntityManager entityManager = state.EntityManager;
        TileSpawnerConfig config = SystemAPI.GetSingleton<TileSpawnerConfig>();
        WorldData worldData = WorldDataManager.worldData;
        ref MapData mapData = ref worldData.map;

        NativeArray<Entity> tiles = entityManager.Instantiate(config.tilePrefab, (int)mapData.tiles.LongLength, Allocator.Temp);

        for (int x = 0; x < mapData.tiles.GetLength(0); x++)
        {
            for (int y = 0; y < mapData.tiles.GetLength(0); y++)
            {
                Entity entity = tiles[x * mapData.tiles.GetLength(0) + y];

                entityManager.SetComponentData(entity, LocalTransform.FromPosition(2 * x, 1, 2 * y));

                entityManager.AddComponent(entity, new ComponentTypeSet(mapData.tiles[x, y].components.Keys.ToArray()));
                foreach (var component in mapData.tiles[x, y].components)
                {

                    SystemAPI.SetComponent(entity, (MapTileComponent)component.Value);
                    entityManager.SetComponentData()
                }
            }
        }
        tiles.Dispose();
    }
}