using System;
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
        var tilePrefab = SystemAPI.GetSingleton<TilePrefab>();
        NativeArray<ComponentType> typesToIgnore = state.EntityManager.GetChunk(tilePrefab.tilePrefab).Archetype.GetComponentTypes();

        HashSet<ComponentType> typesToIgnoreSet = new();
        foreach (var type in typesToIgnore)
            typesToIgnoreSet.Add(type);

        WorldDataSystem.worldData.map.tiles = new Tile[WorldDataSystem.worldData.map.tiles.GetLength(0), WorldDataSystem.worldData.map.tiles.GetLength(1)];

        int serializedTilesCount = 0;
        foreach ((MapTileComponent mapTileComponent, Entity entity) in SystemAPI.Query<MapTileComponent>().WithEntityAccess())
        {
            int2 index = mapTileComponent.pos;
            Debug.Assert(WorldDataSystem.worldData.map.tiles[index.x, index.y].components == null, $"{index}");

            Tile tile = new(mapTileComponent);
            NativeArray<ComponentType> componentTypes = state.EntityManager.GetChunk(entity).Archetype.GetComponentTypes(Allocator.TempJob);

            foreach (var componentType in componentTypes)
            {
                // Ignore all prefab types (rendering & transform) and the mapTileComponent (which has already been saved)
                if (typesToIgnore.Contains(componentType) || componentType == typeof(MapTileComponent)) continue;

                // Local method to simplify code
                EntityManager entityManager = state.EntityManager;
                void AddComponentData<T>() where T : unmanaged, IComponentData
                {
                    // If the component is enableable, check if it is enabled. Else set it to true 
                    bool isEnabled = !componentType.IsEnableable || entityManager.IsComponentEnabled(entity, componentType);
                    tile.components.Add((entityManager.GetComponentData<T>(entity), isEnabled));
                }
                void AddTagComponent<T>() where T : unmanaged, IComponentData
                {
                    bool isEnabled = !componentType.IsEnableable || entityManager.IsComponentEnabled(entity, componentType);
                    tile.components.Add((new T(), isEnabled));
                }

                if (componentType == typeof(AgingTile)) AddComponentData<AgingTile>();
                else if (componentType == typeof(ElectricityProducer)) AddComponentData<ElectricityProducer>();
                else if (componentType == typeof(ElectricityConsumer)) AddComponentData<ElectricityConsumer>();

                else if (componentType == typeof(HasElectricityTag)) AddTagComponent<HasElectricityTag>();
                else if (componentType == typeof(IsConnectedTag)) AddTagComponent<IsConnectedTag>();
                else if (componentType == typeof(ActiveTileTag)) AddTagComponent<ActiveTileTag>();

                else
                    Debug.LogError($"Component of type {componentType} will not be serialized but also isn't present in {typesToIgnore}");

            }
            WorldDataSystem.worldData.map.tiles[index.x, index.y] = tile;

            componentTypes.Dispose();

            serializedTilesCount++;
        }

        if (serializedTilesCount < WorldDataSystem.worldData.map.tiles.Length)
            throw new($"Too few tiles found (Found {serializedTilesCount}, expected {WorldDataSystem.worldData.map.tiles.Length})");

        state.EntityManager.DestroyEntity(SystemAPI.GetSingletonEntity<EntityGridHolder>());
    }
}
