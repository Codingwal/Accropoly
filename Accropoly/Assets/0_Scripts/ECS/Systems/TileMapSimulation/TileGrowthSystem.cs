using Unity.Entities;
using UnityEngine;
using Components;

namespace Systems
{
    /// <summary>
    /// Handle tree growth
    /// </summary>
    public partial struct TileGrowthSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Tags.RunGame>();
            state.RequireForUpdate<GrowingTile>();
            state.RequireForUpdate<ConfigComponents.TileGrowing>();
        }
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<ConfigComponents.TileGrowing>();

            new TileGrowthJob()
            {
                ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged),
                deltaTime = Time.deltaTime,
                maxAge = config.maxAge,
                newTileType = config.newTileType,
            }.Schedule();
        }
        private partial struct TileGrowthJob : IJobEntity
        {
            public EntityCommandBuffer ecb;
            public float deltaTime;
            public float maxAge;
            public TileType newTileType;
            public void Execute(ref GrowingTile growingTile, ref Tile tile, in Entity entity)
            {
                growingTile.age += deltaTime;

                if (growingTile.age > maxAge)
                {
                    tile.tileType = newTileType;
                    MaterialsAndMeshesHolder.UpdateMeshAndMaterial(entity, tile.tileType);

                    ecb.RemoveComponent<GrowingTile>(entity);
                }
            }
        }
    }
}