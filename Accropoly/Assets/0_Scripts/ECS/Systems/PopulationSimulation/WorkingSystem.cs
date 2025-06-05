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
        var gameInfo = SystemAPI.GetSingleton<GameInfo>();
        int hours = gameInfo.time.hours;

        if (hours == 3)
        {
            Entities.ForEach((ref Worker worker, in Person person) =>
            {
                if (worker.employer.Equals(new(-1, -1))) return; // skip unemployed people
                worker.timeToWork = PathfindingSystem.CalculateTravelTime(person.homeTile, worker.employer, entityGrid);

                if (worker.timeToWork == -1)
                    Debug.LogWarning($"Couldn't find path to work (travelTime=-1)! (employer={worker.employer}, homeTile={person.homeTile})");
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
            else if (hours >= 4)
            {
                if (worker.employer.Equals(new(-1, -1))) return; // Skip unemployed people
                if (worker.timeToWork == -1) return; // Skip people without valid path to work
                if (pos.Equals(worker.employer)) return; // Skip people that are already at work

                if (gameInfo.time.TimeOfDayInSeconds + worker.timeToWork >= WorldTime.HoursToSeconds(8))
                {
                    traveller.destination = worker.employer;
                    ecb.SetComponentEnabled<WantsToTravel>(entity, true);
                }
            }
        }).Schedule();
    }
}
