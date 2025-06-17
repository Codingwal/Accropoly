using Unity.Entities;
using Components;
using Tags;
using UnityEngine;

namespace Systems
{
    /// <summary>
    /// Handle tree growth
    /// </summary>
    public partial class TileGrowthSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<RunGame>();
            RequireForUpdate<GrowingTile>();
            RequireForUpdate<ConfigComponents.TileGrowing>();
        }
        protected override void OnUpdate()
        {
            var config = SystemAPI.GetSingleton<ConfigComponents.TileGrowing>();
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            float deltaTime = SystemAPI.GetSingleton<GameInfo>().deltaTime;

            Entities.WithAll<ActiveTile>().ForEach((Entity entity, ref Tile tile, ref GrowingTile growingTile) =>
            {
                growingTile.age += deltaTime;

                if (tile.tileType == TileType.Sapling && growingTile.age >= config.maxAge1)
                {
                    tile.tileType = TileType.GrowingForest;
                    ecb.AddComponent<NewTile>(entity);
                }
                else if (tile.tileType == TileType.GrowingForest && growingTile.age >= config.maxAge2)
                {
                    tile.tileType = TileType.Forest;
                    ecb.RemoveComponent<GrowingTile>(entity);
                    ecb.AddComponent<NewTile>(entity);
                }
            }).Schedule();
        }
    }
}