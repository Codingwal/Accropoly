using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public partial class EmployementSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate(GetEntityQuery(typeof(Worker), typeof(SearchesSpaceTag)));
        RequireForUpdate(GetEntityQuery(typeof(Employer), typeof(HasSpaceTag)));
    }
    protected override void OnUpdate()
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        NativeArray<Entity> employerEntities = GetEntityQuery(typeof(Employer), typeof(HasSpaceTag)).ToEntityArray(Allocator.TempJob);
        NativeArray<Employer> employers = GetEntityQuery(typeof(Employer), typeof(HasSpaceTag)).ToComponentDataArray<Employer>(Allocator.TempJob);
        NativeArray<MapTileComponent> employerMapTileComponents = GetEntityQuery(typeof(Employer), typeof(HasSpaceTag), typeof(MapTileComponent)).ToComponentDataArray<MapTileComponent>(Allocator.TempJob);
        NativeArray<int> index = new(1, Allocator.TempJob);

        Entities.WithAll<SearchesSpaceTag>().ForEach((Entity entity, ref Worker worker) =>
        {
            if (index[0] >= employers.Length) return;

            Employer employer = employers[index[0]];

            employer.freeSpace--;
            employers[index[0]] = employer; // Save the updated employer value
            ecb.SetComponent(employerEntities[index[0]], employer); // Save the updated employer value
            worker.employer = employerMapTileComponents[index[0]].pos; // Store the employer position
            ecb.RemoveComponent<SearchesSpaceTag>(entity); // The worker is no longer searching

            if (employer.freeSpace == 0)
            {
                ecb.RemoveComponent<HasSpaceTag>(employerEntities[index[0]]);
                index[0]++; // Move to next employer
            }

        }).WithDisposeOnCompletion(employerEntities).WithDisposeOnCompletion(employers).WithDisposeOnCompletion(employerMapTileComponents).WithDisposeOnCompletion(index).Schedule();
    }
}
