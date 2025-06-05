using Unity.Entities;

namespace Systems
{
    /// <summary>
    /// Delete the NewTile tag from tiles after they have had it for a frame
    /// (This frame is used for initialization)
    /// </summary>
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