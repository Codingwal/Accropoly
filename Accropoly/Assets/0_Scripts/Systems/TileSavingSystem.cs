using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public partial struct TileSavingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SaveGameTag>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        Debug.Log("Saving tiles");

        var tilePrefab = SystemAPI.GetSingleton<TilePrefab>();
        NativeArray<ComponentType> typesToIgnore = state.EntityManager.GetChunk(tilePrefab.tilePrefab).Archetype.GetComponentTypes();

        HashSet<ComponentType> typesToIgnoreSet = new();
        foreach (var type in typesToIgnore)
            typesToIgnoreSet.Add(type);

        JobHandle jobHandle = new Job
        {
            entityManager = state.EntityManager,
            tiles = WorldDataSystem.worldData.map.tiles,
            typesToIgnore = typesToIgnoreSet,
        }.ScheduleParallel(state.Dependency);

        jobHandle.Complete();
    }
    private partial struct Job : IJobEntity
    {
        public EntityManager entityManager;
        public Tile[,] tiles;
        public HashSet<ComponentType> typesToIgnore;
        public void Execute(in MapTileComponent mapTileComponent, in Entity entity)
        {
            int2 index = mapTileComponent.pos;

            Tile tile = new(mapTileComponent);
            NativeArray<ComponentType> componentTypes = entityManager.GetChunk(entity).Archetype.GetComponentTypes(Allocator.TempJob);

            foreach (var componentType in componentTypes)
            {
                if (typesToIgnore.Contains(componentType) || componentType == typeof(MapTileComponent)) continue;

                if (componentType == typeof(AgingTile))
                    tile.components.Add(entityManager.GetComponentData<AgingTile>(entity));
                else
                    Debug.LogError($"Component of type {componentType} will not be serialized but also isn't present in {typesToIgnore}");

            }
            tiles[index.x, index.y] = tile;
        }
    }
}
