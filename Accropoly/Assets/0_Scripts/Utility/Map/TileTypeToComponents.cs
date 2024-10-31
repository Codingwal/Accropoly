using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public static class TileTypeToComponents
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
    public static void UpdateEntity(Entity tile, TileType tileType, int2 pos, int rotation)
    {
        EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Add components according to tileType
        List<(IComponentData, bool)> components = GetComponents(tileType, pos, rotation);

        // Get all componentTypes from the components list
        List<ComponentType> componentTypes = new();
        foreach (var (component, _) in components)
            componentTypes.Add(component.GetType());

        // Add all components of the prefab (Transform & Rendering components)
        Entity prefab = em.CreateEntityQuery(typeof(TilePrefab)).GetSingleton<TilePrefab>(); // Get the tilePrefab
        NativeArray<ComponentType> prefabComponentTypes = em.GetChunk(prefab).Archetype.GetComponentTypes(Allocator.Temp);
        foreach (var componentType in prefabComponentTypes)
            if (!(componentType == typeof(Prefab) || componentType == typeof(LinkedEntityGroup))) // Remove prefab components (for example, 'Prefab' exludes the entity from all queries)
                componentTypes.Add(componentType);
        prefabComponentTypes.Dispose();

        EntityArchetype archetype = em.CreateArchetype(componentTypes.ToArray());
        em.SetArchetype(tile, archetype);

        void SetComponentData<T>(IComponentData component, bool enabled) where T : unmanaged, IComponentData
        {
            em.SetComponentData<T>(tile, (T)component);
            if (component is IEnableableComponent)
                em.SetComponentEnabled(tile, typeof(T), enabled);
        }

        foreach (var (component, enabled) in components)
        {
            Type type = component.GetType();
            if (new ComponentType(type).IsZeroSized) // Handle tag components
            {
                if (new ComponentType(type).IsEnableable) em.SetComponentEnabled(tile, type, enabled);
            }
            else if (type == typeof(MapTileComponent)) SetComponentData<MapTileComponent>(component, enabled);
            else if (type == typeof(AgingTile)) SetComponentData<AgingTile>(component, enabled);
            else if (type == typeof(ElectricityProducer)) SetComponentData<ElectricityProducer>(component, enabled);
            else if (type == typeof(ElectricityConsumer)) SetComponentData<ElectricityConsumer>(component, enabled);
            else Debug.LogError($"Unexpected type {type.Name}");
        }
    }
}