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
        Debug.Log("!");

        TileAgeingConfig config = SystemAPI.GetSingleton<TileAgeingConfig>();

        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

        new TileAgingSetupJob()
        {
            ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            random = new(config.seed),
            minAge = config.randomAgeRange.x,
            maxAge = config.randomAgeRange.y,
        }.Schedule();

        new TileAgingJob()
        {
            ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            maxAge = config.maxAge,
            newTileType = config.newTileType,
        }.Schedule();
    }
    private partial struct TileAgingSetupJob : IJobEntity
    {
        public EntityCommandBuffer ecb;
        public Unity.Mathematics.Random random;
        public int minAge;
        public int maxAge;
        public void Execute(NewTileTag tag, in Entity entity)
        {
            ecb.AddComponent(entity, typeof(AgingTile));
            ecb.SetComponent(entity, new AgingTile
            {
                age = random.NextInt(minAge, maxAge)
            });
        }
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
