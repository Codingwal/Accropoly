using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial struct TileAgingSystem : ISystem
{
    EntityQuery query;
    public void OnCreate(ref SystemState state)
    {
        state.RequireAnyForUpdate(state.GetEntityQuery(typeof(AgingTile)), state.GetEntityQuery(typeof(NewTileTag)));
        state.RequireForUpdate<TileAgeingConfig>();
        query = state.GetEntityQuery(typeof(NewTileTag), ComponentType.Exclude<AgingTile>());
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
        public void Execute(ref AgingTile agingTile, ref LocalTransform localTransform, in Entity entity)
        {
            if (agingTile.age > maxAge)
            {
                localTransform.Position.y++;
                ecb.RemoveComponent<AgingTile>(entity);
            }
            else agingTile.age++;
        }
    }
}
