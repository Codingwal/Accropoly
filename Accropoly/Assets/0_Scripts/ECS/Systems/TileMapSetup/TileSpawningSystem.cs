using System;
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

                foreach ((IComponentData, bool) component in tiles[x, y].components)
                {
                    Type type = component.GetType();

                    ecb.AddComponent(entity, type);

                    // Local method to simplify code
                    EntityManager entityManager = state.EntityManager;
                    void AddComponent<T>() where T : unmanaged, IComponentData
                    {
                        ecb.SetComponent(entity, (T)component.Item1);

                        // If the component is enableable, set the enabled state according to the stored value
                        if ((T)component.Item1 is IEnableableComponent)
                            ecb.SetComponentEnabled(entity, typeof(T), component.Item2);
                    }

                    if (type == typeof(MapTileComponent))
                    {
                        ecb.SetComponent(entity, (MapTileComponent)component.Item1);

                        // Update mesh using MapTileComponent.tileType
                        MaterialMeshPair pair = MaterialsAndMeshesHolder.GetMaterialAndMesh(((MapTileComponent)component.Item1).tileType);
                        ecb.SetSharedComponentManaged(entity, new RenderMeshArray(new Material[] { pair.material }, new Mesh[] { pair.mesh }));

                        // Update rotation and position using MapTileComponent.rotation and x, y
                        quaternion rotation = quaternion.EulerXYZ(0, math.radians(((MapTileComponent)component.Item1).rotation), 0);
                        ecb.SetComponent(entity, LocalTransform.FromPositionRotation(2 * new float3(x, 0, y), rotation));
                    }
                    else if (type == typeof(AgingTile)) AddComponent<AgingTile>();
                    else if (type == typeof(ElectricityProducer)) AddComponent<ElectricityProducer>();
                    else if (type == typeof(ElectricityConsumer)) AddComponent<ElectricityConsumer>();
                    else Debug.LogError($"Unexpected type {type.Name}");
                }

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