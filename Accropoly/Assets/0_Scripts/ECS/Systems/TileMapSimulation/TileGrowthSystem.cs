using Unity.Entities;
using Components;
using Tags;
using UnityEngine;
using Unity.Burst;

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
        }
        protected override void OnUpdate()
        {
            new GrowTilesJob
            {
                ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged),
                deltaTime = SystemAPI.GetSingleton<GameInfo>().deltaTime,
                config = ConfigData.tileConfig.tileGrowing,
            }.Schedule();
        }

        [BurstCompile]
        [WithAll(typeof(ActiveTile))]
        private partial struct GrowTilesJob : IJobEntity
        {
            public EntityCommandBuffer ecb;
            public float deltaTime;
            public TileConfig.TileGrowing config;
            public void Execute(Entity entity, ref Tile tile, ref GrowingTile growingTile)
            {
                growingTile.age += deltaTime / 3600;

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
            }
        }
    }
}