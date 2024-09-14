using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct TileSpawningSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TilePrefab>();
        state.RequireForUpdate<LoadGameTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        TilePrefab prefab = SystemAPI.GetSingleton<TilePrefab>();

        WorldData worldData = WorldDataSystem.worldData;
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        DynamicBuffer<EntityBufferElement> buffer;
        {
            Entity entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<EntityGridHolder>(entity);
            buffer = state.EntityManager.AddBuffer<EntityBufferElement>(entity);
        }

        Tile[,] tiles = worldData.map.tiles;
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                Entity entity = state.EntityManager.Instantiate(prefab); // Entity needs to be created on main thread so that a valid value is stored in the buffer 

                ecb.AddComponent(entity, new NewTileTag());

                foreach (var component in tiles[x, y].components)
                {
                    Type type = component.GetType();

                    ecb.AddComponent(entity, type);

                    if (type == typeof(MapTileComponent))
                    {
                        ecb.SetComponent(entity, (MapTileComponent)component);

                        // Update mesh using MapTileComponent.tileType
                        MaterialMeshPair pair = MaterialsAndMeshesHolder.GetMaterialAndMesh(((MapTileComponent)component).tileType);
                        ecb.SetSharedComponentManaged(entity, new RenderMeshArray(new Material[] { pair.material }, new Mesh[] { pair.mesh }));

                        // Update rotation and position using MapTileComponent.rotation and x, y
                        quaternion rotation = quaternion.EulerXYZ(0, math.radians(((MapTileComponent)component).rotation), 0);
                        ecb.SetComponent(entity, LocalTransform.FromPositionRotation(2 * new float3(x, 0, y), rotation));
                    }
                    else if (type == typeof(AgingTile)) ecb.SetComponent(entity, (AgingTile)component);
                    else Debug.LogError($"Unexpected type {type.Name}");
                }
                buffer.Add(entity);
            }
        }
    }
}