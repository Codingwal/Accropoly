using Unity.Entities;
using UnityEngine;
using Components;

namespace Systems
{
    /// <summary>
    /// Handle tree growth
    /// </summary>
    public partial struct TileAgingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Tags.RunGame>();
            state.RequireForUpdate<AgingTile>();
            state.RequireForUpdate<ConfigComponents.TileAgeing>();
        }
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<ConfigComponents.TileAgeing>();

            new TileAgingJob()
            {
                ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged),
                deltaTime = Time.deltaTime,
                maxAge = config.maxAge,
                newTileType = config.newTileType,
            }.Schedule();
        }
        private partial struct TileAgingJob : IJobEntity
        {
            public EntityCommandBuffer ecb;
            public float deltaTime;
            public float maxAge;
            public TileType newTileType;
            public void Execute(ref AgingTile agingTile, ref Tile tile, in Entity entity)
            {
                agingTile.age += deltaTime;

                if (agingTile.age > maxAge)
                {
                    tile.tileType = newTileType;
                    MaterialsAndMeshesHolder.UpdateMeshAndMaterial(entity, tile.tileType);

                    ecb.RemoveComponent<AgingTile>(entity);
                }
            }
        }
    }
}