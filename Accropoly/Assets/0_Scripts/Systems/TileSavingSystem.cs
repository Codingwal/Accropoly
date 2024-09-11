using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public partial struct TileSavingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SaveGameTag>();
    }
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        Debug.Log("Saving tiles");

        var tilePrefab = SystemAPI.GetSingleton<TilePrefab>();
        NativeArray<ComponentType> typesToIgnore = state.EntityManager.GetChunk(tilePrefab.tilePrefab).Archetype.GetComponentTypes();

        HashSet<ComponentType> typesToIgnoreSet = new();
        foreach (var type in typesToIgnore)
            typesToIgnoreSet.Add(type);

        foreach ((RefRO<MapTileComponent> mapTileComponent, Entity entity) in SystemAPI.Query<RefRO<MapTileComponent>>().WithEntityAccess())
        {
            Debug.Log("!");
            
            int2 index = mapTileComponent.ValueRO.pos;

            Tile tile = new(mapTileComponent.ValueRO);
            NativeArray<ComponentType> componentTypes = state.EntityManager.GetChunk(entity).Archetype.GetComponentTypes(Allocator.TempJob);

            foreach (var componentType in componentTypes)
            {
                if (typesToIgnore.Contains(componentType) || componentType == typeof(MapTileComponent)) continue;

                if (componentType == typeof(AgingTile))
                {
                    tile.components.Add(state.EntityManager.GetComponentData<AgingTile>(entity));
                    Debug.LogWarning("!");
                }
                else
                    Debug.LogError($"Component of type {componentType} will not be serialized but also isn't present in {typesToIgnore}");

            }
            WorldDataSystem.worldData.map.tiles[index.x, index.y] = tile;

            componentTypes.Dispose();
        }
    }
}
