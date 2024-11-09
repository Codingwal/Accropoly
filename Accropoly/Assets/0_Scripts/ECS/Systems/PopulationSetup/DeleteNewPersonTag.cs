using Unity.Entities;

[UpdateInGroup(typeof(PreCreationSystemGroup))]
public partial struct DeleteNewPersonTagSystem : ISystem
{
    private EntityQuery newTileTagQuery;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NewTileTag>();
        newTileTagQuery = state.GetEntityQuery(typeof(NewPersonTag));
    }
    public void OnUpdate(ref SystemState state)
    {
        state.EntityManager.RemoveComponent(newTileTagQuery, typeof(NewPersonTag));
    }
}
