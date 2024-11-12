using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public partial class EmployementSystem : SystemBase
{
    private EntityQuery employersWithSpaceQuery;
    protected override void OnCreate()
    {
        RequireForUpdate(GetEntityQuery(typeof(UnemployedTag)));
        employersWithSpaceQuery = GetEntityQuery(typeof(Employer), typeof(HasSpaceTag), typeof(MapTileComponent));
        RequireForUpdate(employersWithSpaceQuery);
    }
    protected override void OnUpdate()
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        NativeArray<Entity> employerEntities = employersWithSpaceQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<Employer> employers = employersWithSpaceQuery.ToComponentDataArray<Employer>(Allocator.TempJob);
        NativeArray<MapTileComponent> employerMapTileComponents = employersWithSpaceQuery.ToComponentDataArray<MapTileComponent>(Allocator.TempJob);
        NativeArray<int> index = new(1, Allocator.TempJob);

        Entities.WithAll<UnemployedTag>().ForEach((Entity entity, ref Worker worker) =>
        {
            if (index[0] >= employers.Length) return;

            Employer employer = employers[index[0]];

            employer.freeSpace--;
            employers[index[0]] = employer; // Save the updated employer value
            ecb.SetComponent(employerEntities[index[0]], employer); // Save the updated employer value
            worker.employer = employerMapTileComponents[index[0]].pos; // Store the employer position

            ecb.RemoveComponent<UnemployedTag>(entity); // The worker is no longer searching

            if (employer.freeSpace <= 0)
            {
                ecb.RemoveComponent<HasSpaceTag>(employerEntities[index[0]]);
                index[0]++; // Move to next employer
            }
        }).WithDisposeOnCompletion(employerEntities).WithDisposeOnCompletion(employers).WithDisposeOnCompletion(employerMapTileComponents).WithDisposeOnCompletion(index).Schedule();
    }
}
