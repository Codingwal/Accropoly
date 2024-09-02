using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateBefore(typeof(TileSpawnerSystem))]
public partial struct DeleteNewTileTagSystem : ISystem
{
    private EntityQuery newTileTagQuery;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NewTileTag>();
        newTileTagQuery = state.GetEntityQuery(typeof(NewTileTag));
    }
    public void OnUpdate(ref SystemState state)
    {
        state.EntityManager.RemoveComponent(newTileTagQuery, typeof(NewTileTag));
    }
}
