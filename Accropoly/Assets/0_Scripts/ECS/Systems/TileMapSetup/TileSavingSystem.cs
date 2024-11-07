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
        // Ignore all rendering and transform related components
        var tilePrefab = SystemAPI.GetSingleton<Prefab>();
        NativeArray<ComponentType> typesToIgnore = state.EntityManager.GetChunk(tilePrefab.tilePrefab).Archetype.GetComponentTypes();

        // Convert to HashSet for faster search
        HashSet<ComponentType> typesToIgnoreSet = new();
        foreach (var type in typesToIgnore)
            typesToIgnoreSet.Add(type);
        typesToIgnore.Dispose();

        // MapTileComponent gets saved outside of the loop, some tags can be regenerated after loading 
        typesToIgnoreSet.Add(typeof(MapTileComponent));
        typesToIgnoreSet.Add(typeof(HasSpaceTag));
        typesToIgnoreSet.Add(typeof(HasElectricityTag));

        WorldDataSystem.worldData.map.tiles = new Tile[WorldDataSystem.worldData.map.tiles.GetLength(0), WorldDataSystem.worldData.map.tiles.GetLength(1)];

        // Foreach tile...
        foreach ((MapTileComponent mapTileComponent, Entity entity) in SystemAPI.Query<MapTileComponent>().WithEntityAccess())
        {
            int2 index = mapTileComponent.pos; // needed to determine where in the 2DArray the tile gets stored

            Tile tile = new(mapTileComponent);
            NativeArray<ComponentType> componentTypes = state.EntityManager.GetChunk(entity).Archetype.GetComponentTypes(Allocator.TempJob);

            foreach (var componentType in componentTypes)
            {
                // Ignore all prefab types (rendering & transform) and the mapTileComponent (which has already been saved)
                if (typesToIgnoreSet.Contains(componentType)) continue;

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
                    // If the component is enableable, check if it is enabled. Else set it to true 
                    bool isEnabled = !componentType.IsEnableable || entityManager.IsComponentEnabled(entity, componentType);
                    tile.components.Add((new T(), isEnabled));
                }

                if (componentType == typeof(AgingTile)) AddComponentData<AgingTile>();
                else if (componentType == typeof(ElectricityProducer)) AddComponentData<ElectricityProducer>();
                else if (componentType == typeof(ElectricityConsumer)) AddComponentData<ElectricityConsumer>();
                else if (componentType == typeof(BuildingConnector)) AddComponentData<BuildingConnector>();
                else if (componentType == typeof(Polluter)) AddComponentData<Polluter>();
                else if (componentType == typeof(Habitat)) AddComponentData<Habitat>();

                else if (componentType == typeof(HasElectricityTag)) AddTagComponent<HasElectricityTag>();
                else if (componentType == typeof(IsConnectedTag)) AddTagComponent<IsConnectedTag>();
                else if (componentType == typeof(ActiveTileTag)) AddTagComponent<ActiveTileTag>();

                else Debug.LogWarning($"Component of type {componentType} will not be serialized but also isn't present in typesToIgnore");

            }
            WorldDataSystem.worldData.map.tiles[index.x, index.y] = tile;

            componentTypes.Dispose();
        }
        state.EntityManager.DestroyEntity(SystemAPI.GetSingletonEntity<EntityGridHolder>());
    }
}
