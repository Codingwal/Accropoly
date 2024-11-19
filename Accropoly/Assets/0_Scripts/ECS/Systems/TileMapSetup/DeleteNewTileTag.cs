using Unity.Entities;

namespace Systems
{
    [UpdateInGroup(typeof(PreCreationSystemGroup))]
    public partial struct DeleteNewTileTag : ISystem
    {
        private EntityQuery newTileTagQuery;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Tags.NewTile>();
            newTileTagQuery = state.GetEntityQuery(typeof(Tags.NewTile));
        }
        public void OnUpdate(ref SystemState state)
        {
            state.EntityManager.RemoveComponent(newTileTagQuery, typeof(Tags.NewTile));
        }
    }
}