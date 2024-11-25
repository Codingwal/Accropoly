using Unity.Entities;
using Components;

namespace Systems
{
    [UpdateInGroup(typeof(CreationSystemGroup))]
    public partial struct DeleteTileToPlace : ISystem
    {
        private EntityQuery tileToPlaceInfoQuery;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Tags.SaveGame>();
            state.RequireForUpdate<TileToPlaceInfo>();

            tileToPlaceInfoQuery = state.GetEntityQuery(typeof(TileToPlaceInfo));
        }
        public void OnUpdate(ref SystemState state)
        {
            state.EntityManager.DestroyEntity(tileToPlaceInfoQuery.GetSingletonEntity());
        }
    }
}