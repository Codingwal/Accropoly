using Unity.Entities;

public partial struct DeleteTileToPlace : ISystem
{
    private EntityQuery tileToPlaceQuery;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SaveGameTag>();
        state.RequireForUpdate<TileToPlace>();

        tileToPlaceQuery = state.GetEntityQuery(typeof(TileToPlace));
    }
    public void OnUpdate(ref SystemState state)
    {
        state.EntityManager.DestroyEntity(tileToPlaceQuery.GetSingletonEntity());
    }
}
