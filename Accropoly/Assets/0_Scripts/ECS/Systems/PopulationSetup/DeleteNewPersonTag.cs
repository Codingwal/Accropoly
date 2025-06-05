using Unity.Entities;

namespace Systems
{
    /// <summary>
    /// Delete the NewPerson tag from people after they have had it for a frame
    /// (This frame is used for initialization)
    /// </summary>
    [UpdateInGroup(typeof(PreCreationSystemGroup))]
    public partial struct DeleteNewPersonTag : ISystem
    {
        private EntityQuery newTileTagQuery;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Tags.NewPerson>();
            newTileTagQuery = state.GetEntityQuery(typeof(Tags.NewPerson));
        }
        public void OnUpdate(ref SystemState state)
        {
            state.EntityManager.RemoveComponent(newTileTagQuery, typeof(Tags.NewPerson));
        }
    }
}