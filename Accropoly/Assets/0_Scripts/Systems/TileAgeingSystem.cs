using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial struct TileAgingSystem : ISystem
{
    EntityQuery query;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RunGameTag>();
        // state.RequireAnyForUpdate(state.GetEntityQuery(typeof(AgingTile)), state.GetEntityQuery(typeof(NewTileTag)));
        // state.RequireForUpdate<NewTileTag>();
        query = state.GetEntityQuery(typeof(NewTileTag), ComponentType.Exclude<AgingTile>());
    }
    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

        new TileAgingSetupJob()
        {
            ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            random = new(100),  // Seed is arbitrary
            minAge = 0,
            maxAge = 3000,
        }.Schedule();

        new TileAgingJob()
        {
            ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            maxAge = 5000,
            newTileType = TileType.Forest,
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
            // agingTile.age++;
            if (agingTile.age > maxAge)
            {
                localTransform.Position.y++;
                ecb.RemoveComponent<AgingTile>(entity);
            }
            else agingTile.age++;
        }
    }
}
