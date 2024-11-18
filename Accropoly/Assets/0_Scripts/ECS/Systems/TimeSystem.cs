using Unity.Entities;

public partial struct TimeSystem : ISystem
{
    private EntityQuery newDayTagQuery;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RunGameTag>();
        state.RequireForUpdate<ConfigComponents.Time>();
        newDayTagQuery = state.GetEntityQuery(typeof(NewDayTag));
    }
    public void OnUpdate(ref SystemState state)
    {
        state.CompleteDependency(); // Important because TaxSystem writes to GameInfo and GetSingleton doesn't complete dependencies but throws an error

        var config = SystemAPI.GetSingleton<ConfigComponents.Time>();
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        ecb.DestroyEntity(newDayTagQuery, EntityQueryCaptureMode.AtRecord); // Delete old tag if present

        GameInfo gameInfo = SystemAPI.GetSingleton<GameInfo>();
        gameInfo.time.seconds += SystemAPI.Time.DeltaTime;

        if (gameInfo.time.seconds > config.secondsPerDay)
        {
            gameInfo.time.seconds -= config.secondsPerDay;
            gameInfo.time.day++;
            ecb.CreateEntity(state.EntityManager.CreateArchetype(typeof(NewDayTag))); // Create NewDayTag singleton
        }

        SystemAPI.SetSingleton(gameInfo);
    }
}
