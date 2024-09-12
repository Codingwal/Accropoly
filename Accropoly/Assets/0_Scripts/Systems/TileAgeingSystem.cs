using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public partial struct TileAgingSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AgingTile>();
        state.RequireForUpdate<TileAgeingConfig>();
    }
    public void OnUpdate(ref SystemState state)
    {
        TileAgeingConfig config = SystemAPI.GetSingleton<TileAgeingConfig>();

        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

        new TileAgingJob()
        {
            ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            maxAge = config.maxAge,
            newTileType = config.newTileType,
        }.Schedule();
    }
    private partial struct TileAgingJob : IJobEntity
    {
        public EntityCommandBuffer ecb;
        public int maxAge;
        public TileType newTileType;
        public void Execute(ref AgingTile agingTile, ref MapTileComponent mapTileComponent, in Entity entity)
        {
            if (agingTile.age > maxAge)
            {
                mapTileComponent.tileType = newTileType;
                MaterialsAndMeshesHolder.UpdateMeshAndMaterial(entity, mapTileComponent.tileType);

                ecb.RemoveComponent<AgingTile>(entity);
            }
            else agingTile.age++;
        }
    }
}
