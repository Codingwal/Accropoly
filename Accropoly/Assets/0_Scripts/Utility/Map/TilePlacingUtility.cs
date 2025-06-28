using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Components;
using Tags;

public static class TilePlacingUtility
{
    public static List<(IComponentData, bool)> GetComponents(TileType tileType, int2 pos, Direction rotation)
    {
        var tileGrowingConfig = ECSUtility.GetSingleton<ConfigComponents.TileGrowing>();
        System.Random rnd = new();
        List<(IComponentData, bool)> components = tileType switch
        {
            TileType.Plains => new() { },
            TileType.Sapling => new() { (new GrowingTile { age = rnd.Next(tileGrowingConfig.maxAge1) }, true) },
            TileType.Forest => new() { },
            TileType.House => new() { (new Habitat {totalSpace = rnd.Next(2, 6)}, true),
                                      (new ElectricityConsumer { consumption = 2, disableIfElectroless = false }, true),
                                      (new Polluter { pollution = 3 }, true), (new IsConnected(), false) },
            TileType.SolarPanel => new() { (new ElectricityProducer { production = 10 }, true), (new Polluter { pollution = 1 }, true),
                                           (new Employer{totalSpace = 1}, true) },
            TileType.Street => new() { (new ConnectingTile(ConnectingTileGroup.Street), true), (new BuildingConnector(), true), (new TransportTile(10), true) },
            TileType.Lake => new() { (new ConnectingTile(ConnectingTileGroup.Lake), true) },
            TileType.River => new() { (new ConnectingTile(ConnectingTileGroup.River), true) },
            TileType.Hut => new() { (new Habitat { totalSpace = rnd.Next(1, 3) }, true) },
            TileType.Office => new() { (new ElectricityConsumer { consumption = 5, disableIfElectroless = true }, true),
                                       (new Employer { totalSpace = 10 }, true),
                                       (new Polluter { pollution = 5 }, true), (new IsConnected(), false) },
            TileType.WindTurbine => new() { (new ElectricityProducer { production = 50 }, true), (new Polluter { pollution = 2 }, true),
                                           (new Employer {totalSpace = 2}, true) },
            TileType.GrowingForest => new() { (new GrowingTile { age = rnd.Next(tileGrowingConfig.maxAge1, tileGrowingConfig.maxAge2) }, true) },
            TileType.Bitumen => new() { },
            TileType.CityStreet => new() { (new ConnectingTile(ConnectingTileGroup.Street), true), (new BuildingConnector(), true), (new TransportTile(10), true) },
            TileType.ForestStreet => new() { (new ConnectingTile(ConnectingTileGroup.Street), true), (new BuildingConnector(), true), (new TransportTile(10), true) },
            _ => throw new($"Missing componentTypes for tileType {tileType}")
        };
        components.Add((new Tile { tileType = tileType, pos = pos, rotation = rotation }, true));
        components.Add((new ActiveTile(), false));
        components.Add((new NewTile(), true));
        return components;
    }
    public static void UpdateEntity(Entity tile, List<(IComponentData, bool)> components, EntityCommandBuffer ecb)
    {
        EntityManager em = ECSUtility.EntityManager;

        // Get all componentTypes from the components list
        List<ComponentType> componentTypes = new();
        foreach (var (component, _) in components)
            componentTypes.Add(component.GetType());

        // Add all components of the prefab (Transform & Rendering components)
        var prefab = ECSUtility.GetSingleton<ConfigComponents.PrefabEntity>(); // Get the tilePrefab
        NativeArray<ComponentType> prefabComponentTypes = em.GetChunk(prefab).Archetype.GetComponentTypes(Allocator.Temp);

        foreach (var componentType in prefabComponentTypes)
            if (!(componentType == typeof(Prefab) || componentType == typeof(LinkedEntityGroup))) // Remove prefab components (for example, 'Prefab' exludes the entity from all queries)
                componentTypes.Add(componentType);
        prefabComponentTypes.Dispose();

        // Moving the archetype keeps values of components that were already present (rendering components, Scene, ...)
        EntityArchetype archetype = em.CreateArchetype(componentTypes.ToArray());
        em.SetArchetype(tile, archetype);

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
            else if (type == typeof(Tile)) SetComponentData<Tile>(component, enabled);
            else if (type == typeof(GrowingTile)) SetComponentData<GrowingTile>(component, enabled);
            else if (type == typeof(ElectricityProducer)) SetComponentData<ElectricityProducer>(component, enabled);
            else if (type == typeof(ElectricityConsumer)) SetComponentData<ElectricityConsumer>(component, enabled);
            else if (type == typeof(ConnectingTile)) SetComponentData<ConnectingTile>(component, enabled);
            else if (type == typeof(Polluter)) SetComponentData<Polluter>(component, enabled);
            else if (type == typeof(Habitat)) SetComponentData<Habitat>(component, enabled);
            else if (type == typeof(Employer)) SetComponentData<Employer>(component, enabled);
            else if (type == typeof(TransportTile)) SetComponentData<TransportTile>(component, enabled);
            else Debug.LogError($"Unexpected type {type.Name}");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="oldType"></param>
    /// <param name="newType">The TileType selected in the menu. Might not be the TileType that's actually placed.</param>
    /// <returns>(The TileType to place, cost)</returns>
    public static (TileType, int) GetPlacingData(TileType oldType, TileType newType)
    {
        (TileType, int) INVALID = (TileType.None, -1);

        Debug.Assert(oldType != TileType.None && newType != TileType.None);

        // Water can't be replaced
        if (oldType == TileType.River || oldType == TileType.Lake)
            return INVALID;

        // Handle placing on empty tile
        if (oldType == TileType.Plains)
        {
            return newType switch
            {
                TileType.River => (TileType.River, 100), // TEMPORARY
                TileType.Lake => (TileType.Lake, 10),

                TileType.Sapling => (TileType.Sapling, 10),
                TileType.House => (TileType.House, 200),
                TileType.SolarPanel => (TileType.SolarPanel, 50),
                TileType.Street => (TileType.Street, 50),
                TileType.Hut => (TileType.Hut, 50),
                TileType.Office => (TileType.Office, 500),
                TileType.WindTurbine => (TileType.WindTurbine, 1000),
                TileType.Bitumen => (TileType.Bitumen, 20),

                _ => INVALID
            };
        }

        switch (newType)
        {
            case TileType.Plains:
                return (TileType.Plains, 5);
            case TileType.Sapling:
                if (oldType == TileType.House)
                    return (TileType.Hut, 10);
                else
                    return INVALID;
            case TileType.House:
                if (oldType == TileType.Forest)
                    return (TileType.Hut, 50);
                else
                    return INVALID;
            case TileType.Street:
                if (oldType == TileType.Bitumen)
                    return (TileType.CityStreet, 20);
                else if (oldType == TileType.Forest)
                    return (TileType.ForestStreet, 20);
                else
                    return INVALID;
            default:
                return INVALID;
        }
    }
}