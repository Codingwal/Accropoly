using Components;
using Systems;
using Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class WorkingSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<RunGame>();
    }
    protected override void OnUpdate()
    {
        Dependency.Complete();

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        var entityGrid = SystemAPI.GetBuffer<EntityBufferElement>(SystemAPI.GetSingletonEntity<EntityGridHolder>());
        var timeConfig = SystemAPI.GetSingleton<ConfigComponents.Time>();
        int hours = SystemAPI.GetSingleton<GameInfo>().time.hours;

        if (hours == 0)
        {
            Entities.ForEach((ref Worker worker, in Person person) =>
            {
                worker.timeToWork = PathfindingSystem.CalculateTravelTime(person.homeTile, worker.employer, entityGrid);
            }).Schedule();
        }

        Entities.WithDisabled<WantsToTravel, Travelling>().ForEach((Entity entity, ref Traveller traveller, in LocalTransform transform, in Person person, in Worker worker) =>
        {
            int2 pos = (int2)math.round(transform.Position.xz / 2);
            if (hours >= 16)
            {
                if (pos.Equals(person.homeTile)) return; // Skip people that are already at home

                traveller.destination = person.homeTile;
                ecb.SetComponentEnabled<WantsToTravel>(entity, true);

            }
            else if (hours >= 3)
            {
                if (worker.employer.Equals(-1)) return; // Skip unemployed people
                if (pos.Equals(worker.employer)) return; // Skip people that are already at work

                if (hours + worker.timeToWork * 24 / timeConfig.secondsPerDay >= 8)
                {
                    traveller.destination = worker.employer;
                    ecb.SetComponentEnabled<WantsToTravel>(entity, true);
                }
            }
        }).Schedule();
    }
}
