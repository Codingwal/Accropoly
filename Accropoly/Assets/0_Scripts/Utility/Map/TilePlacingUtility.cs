using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public static class TilePlacingUtility
{
    public static List<(IComponentData, bool)> GetComponents(TileType tileType, int2 pos, int rotation)
    {
        List<(IComponentData, bool)> components = tileType switch
        {
            TileType.Plains => new() { },
            TileType.Sapling => new() { (new AgingTile { age = new Unity.Mathematics.Random(1).NextFloat(0, 10) }, true) },
            TileType.Forest => new() { },
            _ => throw new($"Missing componentTypes for tileType {tileType}"),
        };
        components.Add((new MapTileComponent { tileType = tileType, pos = pos, rotation = rotation }, true));
        components.Add((new ActiveTileTag(), true));
        components.Add((new NewTileTag(), true));
        return components;
    }
    public static Entity CreateTile(List<(IComponentData, bool)> components, EntityCommandBuffer ecb)
    {
        Debug.Log("!");

        EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Get all componentTypes from the components list
        List<ComponentType> componentTypes = new();
        foreach (var (component, _) in components)
            componentTypes.Add(component.GetType());

        // Add all components of the prefab (Transform & Rendering components)
        Entity prefab = em.CreateEntityQuery(typeof(TilePrefab)).GetSingleton<TilePrefab>(); // Get the tilePrefab
        NativeArray<ComponentType> prefabComponentTypes = em.GetChunk(prefab).Archetype.GetComponentTypes(Allocator.Temp);
        foreach (var componentType in prefabComponentTypes)
            if (!(componentType == typeof(Prefab) || componentType == typeof(LinkedEntityGroup))) // Skip prefab components (for example, 'Prefab' exludes the entity from all queries)
                componentTypes.Add(componentType);
        prefabComponentTypes.Dispose();

        EntityArchetype archetype = em.CreateArchetype(componentTypes.ToArray());
        Entity tile = em.CreateEntity(archetype);
        Debug.Log("Created tile: " + componentTypes.Count);

        // Local helper function
        void SetComponentData<T>(IComponentData component, bool enabled) where T : unmanaged, IComponentData
        {
            ecb.SetComponent<T>(tile, (T)component);
            if (component is IEnableableComponent)
                ecb.SetComponentEnabled(tile, typeof(T), enabled);
        }

        // Set values for all components
        foreach (var (component, enabled) in components)
        {
            Type type = component.GetType();
            if (new ComponentType(type).IsZeroSized) // Handle tag components
            {
                if (new ComponentType(type).IsEnableable) ecb.SetComponentEnabled(tile, type, enabled);
            }
            else if (type == typeof(MapTileComponent)) SetComponentData<MapTileComponent>(component, enabled);
            else if (type == typeof(AgingTile)) SetComponentData<AgingTile>(component, enabled);
            else if (type == typeof(ElectricityProducer)) SetComponentData<ElectricityProducer>(component, enabled);
            else if (type == typeof(ElectricityConsumer)) SetComponentData<ElectricityConsumer>(component, enabled);
            else Debug.LogError($"Unexpected type {type.Name}");
        }

        return tile;
    }
}