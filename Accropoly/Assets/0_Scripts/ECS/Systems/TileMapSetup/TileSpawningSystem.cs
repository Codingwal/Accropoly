using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(CreationSystemGroup))]
public partial struct TileSpawningSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PrefabEntity>();
        state.RequireForUpdate<LoadGameTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var prefab = SystemAPI.GetSingleton<PrefabEntity>();

        WorldData worldData = WorldDataSystem.worldData;
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        TileGridUtility.CreateEntityGridBuffer();

        Tile[,] tiles = worldData.map.tiles;
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                // Entity needs to be created on main thread so that a valid value is stored in the buffer 
                // Instantiate not CreateEntity because we need the prefab values copied (rendering stuff, sceneTag, ...)
                Entity entity = state.EntityManager.Instantiate(prefab);

                // Add all serialized components with their value to the entity
                TilePlacingUtility.UpdateEntity(entity, tiles[x, y].components, ecb);

                // Get MapTileComponent
                MapTileComponent mapTileComponent = new();
                foreach (var (component, _) in tiles[x, y].components)
                    if (component.GetType() == typeof(MapTileComponent))
                        mapTileComponent = (MapTileComponent)component;

                // Set LocalTransform of the new tile using the MapTileComponent data
                quaternion rotation = quaternion.EulerXYZ(0, math.radians((uint)mapTileComponent.rotation * 90), 0);
                ecb.SetComponent(entity, LocalTransform.FromPositionRotation(2 * new float3(x, 0, y), rotation));

                // Set mesh using MapTileComponent.tileType
                MaterialMeshPair pair = MaterialsAndMeshesHolder.GetMaterialAndMesh(mapTileComponent.tileType);
                ecb.SetSharedComponentManaged(entity, new RenderMeshArray(new Material[] { pair.material }, new Mesh[] { pair.mesh }));

                // Store the entity in a buffer for future access
                TileGridUtility.GetEntityGrid().Add(entity);
            }
        }
    }
}