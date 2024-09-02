using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateBefore(typeof(TileSpawnerSystem))]
public partial struct DeleteNewTileTagSystem : ISystem
{
    private EntityQuery newTileTagQuery;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RunGameTag>();
        // state.RequireForUpdate<NewTileTag>();
        newTileTagQuery = state.GetEntityQuery(typeof(NewTileTag));
    }
    public void OnUpdate(ref SystemState state)
    {
        state.EntityManager.RemoveComponent(newTileTagQuery, typeof(NewTileTag));

        // var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        // new DeleteNewTileTagJob()
        // {
        //     ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged),
        // }.Schedule();
    }
    private partial struct DeleteNewTileTagJob : IJobEntity
    {
        public EntityCommandBuffer ecb;
        public void Execute(NewTileTag tag, in Entity entity)
        {
            ecb.RemoveComponent(entity, typeof(NewTileTag));
        }
    }
}
