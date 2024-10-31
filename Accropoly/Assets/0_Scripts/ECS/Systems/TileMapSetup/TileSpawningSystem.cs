using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
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
                buffer.Add(entity);

                ecb.AddComponent(entity, new NewTileTag());

                TilePlacingUtility.UpdateEntity(entity, tiles[x, y].components);

                foreach (var tag in tiles[x, y].tags)
                {
                    ecb.AddComponent(entity, tag.Item1);
                    if (tag.Item1.IsEnableable)
                        ecb.SetComponentEnabled(entity, tag.Item1, tag.Item2);
                }
            }
        }
    }
}