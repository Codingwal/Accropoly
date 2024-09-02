using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial struct TileAgingSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireAnyForUpdate(state.GetEntityQuery(typeof(AgingTile)), state.GetEntityQuery(typeof(NewTileTag)));
        // state.RequireForUpdate<NewTileTag>();
    }
    public void OnUpdate(ref SystemState state)
    {
        Debug.Log("Modify");
        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

        new TileAgingSetupJob()
        {
            ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            minAge = 0,
            maxAge = 500,
        }.Schedule(state.GetEntityQuery(typeof(NewTileTag), ComponentType.Exclude<AgingTile>()));

        new TileAgingJob()
        {
            ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
            maxAge = 10000,
            newTileType = TileType.Forest,
        }.ScheduleParallel();
    }
    private partial struct TileAgingSetupJob : IJobEntity
    {
        public EntityCommandBuffer ecb;
        public int minAge;
        public int maxAge;
        public void Execute(NewTileTag tag, in Entity entity)
        {
            Debug.Log("!");
            ecb.AddComponent(entity, typeof(AgingTile));
            ecb.SetComponent(entity, new AgingTile
            {
                age = Random.Range(minAge, maxAge)
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
            agingTile.age++;
            // if (agingTile.age > maxAge)
            // {
            // localTransform.Position.y++;
            // ecb.RemoveComponent<AgingTile>(entity);
            // }
            // else agingTile.age++;
        }
    }
}
