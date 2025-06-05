using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Components;
using Tags;

namespace Systems
{
    /// <summary>
    /// Save tile map data
    /// </summary>
    [UpdateInGroup(typeof(CreationSystemGroup))]
    public partial struct TileSavingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SaveGame>();
        }
        public void OnUpdate(ref SystemState state)
        {
            // Ignore all rendering and transform related components
            var prefab = SystemAPI.GetSingleton<ConfigComponents.PrefabEntity>();
            NativeArray<ComponentType> typesToIgnore = state.EntityManager.GetChunk(prefab.prefab).Archetype.GetComponentTypes();

            // Convert to HashSet for faster search
            HashSet<ComponentType> typesToIgnoreSet = new();
            foreach (var type in typesToIgnore)
                typesToIgnoreSet.Add(type);
            typesToIgnore.Dispose();

            // Tile component gets saved outside of the loop, some tags and components can be regenerated after loading 
            typesToIgnoreSet.Add(typeof(Tile));
            typesToIgnoreSet.Add(typeof(HasSpace));
            typesToIgnoreSet.Add(typeof(HasElectricity));
            typesToIgnoreSet.Add(typeof(BillboardOwner));

            WorldDataSystem.worldData.map.tiles = new TileData[WorldDataSystem.worldData.map.tiles.GetLength(0), WorldDataSystem.worldData.map.tiles.GetLength(1)];

            // Foreach tile...
            foreach ((Tile mapTileComponent, Entity entity) in SystemAPI.Query<Tile>().WithEntityAccess())
            {
                int2 index = mapTileComponent.pos; // needed to determine where in the 2DArray the tile gets stored

                TileData tile = new(mapTileComponent);
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
                    void AddTag<T>() where T : unmanaged, IComponentData
                    {
                        // If the component is enableable, check if it is enabled. Else set it to true 
                        bool isEnabled = !componentType.IsEnableable || entityManager.IsComponentEnabled(entity, componentType);
                        tile.components.Add((new T(), isEnabled));
                    }

                    if (componentType == typeof(AgingTile)) AddComponentData<AgingTile>();
                    else if (componentType == typeof(ElectricityProducer)) AddComponentData<ElectricityProducer>();
                    else if (componentType == typeof(ElectricityConsumer)) AddComponentData<ElectricityConsumer>();
                    else if (componentType == typeof(ConnectingTile)) AddComponentData<ConnectingTile>();
                    else if (componentType == typeof(Polluter)) AddComponentData<Polluter>();
                    else if (componentType == typeof(Habitat)) AddComponentData<Habitat>();
                    else if (componentType == typeof(Employer)) AddComponentData<Employer>();
                    else if (componentType == typeof(TransportTile)) AddComponentData<TransportTile>();

                    else if (componentType == typeof(IsConnected)) AddTag<IsConnected>();
                    else if (componentType == typeof(ActiveTile)) AddTag<ActiveTile>();
                    else if (componentType == typeof(BuildingConnector)) AddTag<BuildingConnector>();
                    else if (componentType == typeof(DisabledTile)) AddTag<DisabledTile>();

                    else Debug.LogWarning($"Component of type {componentType} will not be serialized but also isn't present in typesToIgnore");

                }
                WorldDataSystem.worldData.map.tiles[index.x, index.y] = tile;

                componentTypes.Dispose();
            }
            state.EntityManager.DestroyEntity(SystemAPI.GetSingletonEntity<EntityGridHolder>());
        }
    }
}
