using Unity.Entities;
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
        if (Time.timeScale == 0) return;

        TileAgeingConfig config = SystemAPI.GetSingleton<TileAgeingConfig>();

        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

        new TileAgingJob()
        {
            ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
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
        public void Execute(ref AgingTile agingTile, ref MapTileComponent mapTileComponent, in Entity entity)
        {
            agingTile.age += deltaTime;

            if (agingTile.age > maxAge)
            {
                mapTileComponent.tileType = newTileType;
                MaterialsAndMeshesHolder.UpdateMeshAndMaterial(entity, mapTileComponent.tileType);

                ecb.RemoveComponent<AgingTile>(entity);
            }
        }
    }
}
