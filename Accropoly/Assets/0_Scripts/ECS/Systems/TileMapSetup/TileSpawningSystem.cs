using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Components;
using Unity.Rendering;

namespace Systems
{
    /// <summary>
    /// Re-create tile grid using the saved tile map data
    /// </summary>
    [UpdateInGroup(typeof(CreationSystemGroup))]
    public partial struct TileSpawningSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Tags.LoadGame>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var prefab = SystemAPI.GetSingleton<ConfigComponents.PrefabEntity>().tilePrefab;

            WorldData worldData = WorldDataSystem.worldData;

            TileGridUtility.CreateEntityGridBuffer();

            AppearenceSystem appearenceSystem = state.World.GetOrCreateSystemManaged<AppearenceSystem>();

            var tiles = worldData.map.tiles;
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    // Entity needs to be created on main thread so that a valid value is stored in the buffer 
                    // Instantiate not CreateEntity because we need the prefab values copied (rendering stuff, sceneTag, ...)
                    Entity entity = state.EntityManager.Instantiate(prefab);

                    // Add all serialized components with their value to the entity
                    TilePlacingUtility.UpdateEntity(entity, tiles[x, y].components);

                    // Get tile component
                    Tile tile = new();
                    foreach (var (component, _) in tiles[x, y].components)
                        if (component.GetType() == typeof(Tile))
                            tile = (Tile)component;


                    // Set LocalTransform of the new tile using the tile data
                    quaternion rotation = quaternion.EulerXYZ(0, math.radians((uint)tile.rotation * 90), 0);
                    state.EntityManager.SetComponentData(entity, LocalTransform.FromPositionRotation(2 * new float3(x, 0, y), rotation));

                    // The tile meshes are 2 units large -> the render bounds need to be extended from 0.5 to 1
                    state.EntityManager.SetComponentData(entity, new RenderBounds() { Value = new AABB() { Extents = new(1, 1, 1) } });

                    // Store the entity in a buffer for future access (can't store entityGrid bc the ref gets invalidated by structural changes)
                    TileGridUtility.GetEntityGridRW().Add(entity);

                    appearenceSystem.UpdateAppearence(entity, tile.tileType);
                }
            }
        }
    }
}