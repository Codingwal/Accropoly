using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
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
        state.RequireForUpdate<RunGameTag>();

        WorldDataManager.onWorldDataSaving += OnWorldDataSaving;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity prefabHolder = SystemAPI.GetSingletonEntity<TilePrefab>();
        TilePrefab prefab = SystemAPI.GetComponent<TilePrefab>(prefabHolder);
        state.EntityManager.RemoveComponent<TilePrefab>(prefabHolder);

        WorldData worldData = WorldDataManager.worldData;
        EntityCommandBuffer commandBuffer = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        Tile[,] tiles = worldData.map.tiles;
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                Entity entity = commandBuffer.Instantiate(prefab);

                commandBuffer.SetComponent(entity, LocalTransform.FromPosition(new(x - tiles.GetLength(0) / 2, 0, y - tiles.GetLength(1) / 2)));

                commandBuffer.AddComponent(entity, new NewTileTag());

                foreach (var component in tiles[x, y].components)
                {
                    Type type = component.GetType();

                    commandBuffer.AddComponent(entity, type);

                    if (type == typeof(MapTileComponent))
                    {
                        commandBuffer.SetComponent(entity, (MapTileComponent)component);
                        MaterialMeshPair pair = MaterialsAndMeshesHolder.GetMaterialAndMesh(((MapTileComponent)component).tileType);
                        commandBuffer.SetSharedComponentManaged(entity, new RenderMeshArray(new Material[] { pair.material }, new Mesh[] { pair.mesh }));
                    }
                    else Debug.LogError($"Unexpected type {type.Name}");
                }
            }
        }
    }
    public void OnWorldDataSaving(ref WorldData worldData)
    {
        var job = new Job
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager,
            tiles = worldData.map.tiles,
            // typesToIgnore =
        };
        job.Schedule();

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
                if (typesToIgnore.Contains(componentType)) continue;

                if (componentType == typeof(AgingTile))
                    tile.components.Add(entityManager.GetComponentData<AgingTile>(entity));
                else
                    Debug.LogError($"Component of type {componentType} will not be serialized but also isn't present in {typesToIgnore}");

            }
        }
    }
}