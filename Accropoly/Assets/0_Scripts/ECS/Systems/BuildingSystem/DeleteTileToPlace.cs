using Unity.Entities;
using Components;

namespace Systems
{
    [UpdateInGroup(typeof(CreationSystemGroup))]
    public partial struct DeleteTileToPlace : ISystem
    {
        private EntityQuery tileToPlaceQuery;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Tags.SaveGame>();
            state.RequireForUpdate<TileToPlace>();

            tileToPlaceQuery = state.GetEntityQuery(typeof(TileToPlace));
        }
        public void OnUpdate(ref SystemState state)
        {
            state.EntityManager.DestroyEntity(tileToPlaceQuery.GetSingletonEntity());
        }
    }
}