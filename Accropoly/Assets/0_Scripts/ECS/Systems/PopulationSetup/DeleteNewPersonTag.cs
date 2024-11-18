using Unity.Entities;

namespace Systems
{
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