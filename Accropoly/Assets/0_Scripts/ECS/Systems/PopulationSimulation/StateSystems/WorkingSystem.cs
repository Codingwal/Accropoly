using Components;
using Components.WaypointComponents;
using Tags;
using Unity.Collections;
using Unity.Entities;

namespace Systems
{
    public partial class WorkingSystem : SystemBase
    {
        EntityQuery notWorking;
        EntityQuery working;
        protected override void OnCreate()
        {
            notWorking = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Person, Traveller, Worker, FreeTime>()
                .WithNone<Unemployed>()
                .Build(this);

            working = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Person, Working>()
            .Build(this);
        }
        protected override void OnUpdate()
        {
            var gameInfo = SystemAPI.GetSingleton<GameInfo>();
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            if (gameInfo.time.hours >= 7 && gameInfo.time.hours < 16)
            {
                new StartWorkingJob()
                {
                    ecb = ecb,
                }.Schedule(notWorking);
            }
            else
            {
                new StopWorkingJob()
                {
                    ecb = ecb
                }.Schedule(working);
            }
        }

        private partial struct StartWorkingJob : IJobEntity
        {
            public EntityCommandBuffer ecb;
            public void Execute(Entity entity, ref Traveller traveller, in Worker worker)
            {
                ecb.RemoveComponent<FreeTime>(entity);
                ecb.AddComponent<Working>(entity);

                traveller.SetJourneyData(worker.employer, TravelObjects.Standard);
                ecb.SetComponentEnabled<Travelling>(entity, false);
                ecb.SetComponentEnabled<WantsToTravel>(entity, true);
            }
        }
        private partial struct StopWorkingJob : IJobEntity
        {
            public EntityCommandBuffer ecb;
            public void Execute(Entity entity)
            {
                ecb.RemoveComponent<Working>(entity);
                ecb.AddComponent<FreeTime>(entity);
            }
        }
    }
}