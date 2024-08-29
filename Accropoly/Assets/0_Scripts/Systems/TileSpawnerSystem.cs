using System;
using System.Collections.Generic;
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
        state.RequireForUpdate<RunGameTag>();
    }

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

                List<IComponentData> components = mapData.tiles[x, y].components;

                // Add all components specified in the components list at once
                ComponentType[] componentTypes = new ComponentType[components.Count];
                for (int i = 0; i < componentTypes.Length; i++) componentTypes[i] = components[i].GetType();
                entityManager.AddComponent(entity, new ComponentTypeSet(componentTypes));

                // Set the component data 
                foreach (var component in components)
                {
                    Type type = component.GetType();

                    if (type == typeof(MapTileComponent)) SystemAPI.SetComponent(entity, (MapTileComponent)component);
                }
            }
        }
        tiles.Dispose();
    }
}