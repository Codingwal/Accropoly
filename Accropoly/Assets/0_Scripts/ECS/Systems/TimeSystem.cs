using Unity.Entities;

namespace Systems
{
    /// <summary>
    /// Calculate in-Game time using deltaTime
    /// Also manage NewDay tag
    /// </summary>
    public partial struct TimeSystem : ISystem
    {
        private EntityQuery newDayTagQuery;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Tags.RunGame>();
            state.RequireForUpdate<ConfigComponents.Time>();
            newDayTagQuery = state.GetEntityQuery(typeof(Tags.NewDay));
        }
        public void OnUpdate(ref SystemState state)
        {
            state.CompleteDependency(); // Important because TaxSystem writes to GameInfo and GetSingleton doesn't complete dependencies but throws an error

            var config = SystemAPI.GetSingleton<ConfigComponents.Time>();
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            ecb.DestroyEntity(newDayTagQuery, EntityQueryCaptureMode.AtRecord); // Delete old tag if present

            var gameInfo = SystemAPI.GetSingleton<Components.GameInfo>();

            gameInfo.deltaTime = SystemAPI.Time.DeltaTime * config.TimeSpeed;
            gameInfo.time.Advance(gameInfo.deltaTime, out bool newDay);

            if (newDay)
                ecb.CreateEntity(state.EntityManager.CreateArchetype(typeof(Tags.NewDay))); // Create NewDayTag singleton

            SystemAPI.SetSingleton(gameInfo);
        }
    }
}