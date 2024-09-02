using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct TileSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TilePrefab>();
        state.RequireForUpdate<RunGameTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity prefabHolder = SystemAPI.GetSingletonEntity<TilePrefab>();
        TilePrefab prefab = SystemAPI.GetComponent<TilePrefab>(prefabHolder);
        state.EntityManager.RemoveComponent<TilePrefab>(prefabHolder);

        WorldData worldData = WorldDataManager.worldData;
        EntityCommandBuffer commandBuffer = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        Tile[,] tiles = worldData.map.tiles;
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                Entity entity = commandBuffer.Instantiate(prefab);

                commandBuffer.SetComponent(entity, LocalTransform.FromPosition(new(x - tiles.GetLength(0) / 2, 0, y - tiles.GetLength(1) / 2)));

                commandBuffer.AddComponent(entity, new NewTileTag());

                foreach (var component in tiles[x, y].components)
                {
                    Type type = component.GetType();

                    commandBuffer.AddComponent(entity, type);

                    if (type == typeof(MapTileComponent))
                    {
                        commandBuffer.SetComponent(entity, (MapTileComponent)component);
                        MaterialMeshPair pair = MaterialsAndMeshesHolder.GetMaterialAndMesh(((MapTileComponent)component).tileType);
                        commandBuffer.SetSharedComponentManaged(entity, new RenderMeshArray(new Material[] { pair.material }, new Mesh[] { pair.mesh }));
                    }
                    else Debug.LogError($"Unexpected type {type.Name}");
                }
            }
        }
    }
}