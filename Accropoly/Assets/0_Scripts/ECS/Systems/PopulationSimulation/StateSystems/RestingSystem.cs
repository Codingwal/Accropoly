using Components;
using Tags;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

public partial class RestingSystem : SystemBase
{
    EntityQuery notResting;
    EntityQuery resting;
    protected override void OnCreate()
    {
        RequireForUpdate<RunGame>();

        notResting = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Person, Traveller, FreeTime, LocalTransform>()
            .WithNone<Homeless>()
            .Build(this);

        resting = new EntityQueryBuilder(Allocator.Temp)
        .WithAll<Person, Resting>()
        .Build(this);
    }
    protected override void OnUpdate()
    {
        var gameInfo = SystemAPI.GetSingleton<GameInfo>();
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        if (gameInfo.time.hours >= 22 || gameInfo.time.hours < 7)
        {
            new StartRestingJob()
            {
                ecb = ecb,
            }.Schedule(notResting);
        }
        else
        {
            new StopRestingJob()
            {
                ecb = ecb
            }.Schedule(resting);
        }
    }

    private partial struct StartRestingJob : IJobEntity
    {
        public EntityCommandBuffer ecb;
        public void Execute(Entity entity, ref Traveller traveller, in Person person, in LocalTransform transform)
        {
            ecb.RemoveComponent<FreeTime>(entity);
            ecb.AddComponent<Resting>(entity);

            // Return if already at home
            if (transform.Position.xz.Equals(person.homeTile * 2))
                return;

            traveller.SetJourneyData(person.homeTile, Components.WaypointComponents.TravelObjects.Standard);
            ecb.SetComponentEnabled<Travelling>(entity, false);
            ecb.SetComponentEnabled<WantsToTravel>(entity, true);
        }
    }
    private partial struct StopRestingJob : IJobEntity
    {
        public EntityCommandBuffer ecb;
        public void Execute(Entity entity)
        {
            ecb.RemoveComponent<Resting>(entity);
            ecb.AddComponent<FreeTime>(entity);
        }
    }
}
