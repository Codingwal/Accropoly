using Components;
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
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        Entities.WithDisabled<WantsToTravel, Travelling>().ForEach((Entity entity, ref Traveller traveller, in LocalTransform transform, in Person person, in Worker worker) =>
        {
            int2 pos = (int2)math.round(transform.Position.xz / 2);
            Debug.Log($"{transform.Position.xz} -> {pos}");
            if (pos.Equals(person.homeTile))
            {
                if (worker.employer.Equals(-1)) return; // Skip unemployed people
                traveller.destination = worker.employer;
                ecb.SetComponentEnabled<WantsToTravel>(entity, true);
            }
            else
            {
                traveller.destination = person.homeTile;
                ecb.SetComponentEnabled<WantsToTravel>(entity, true);
            }
        }).Schedule();
    }
}
