using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Components;
using Tags;
using Unity.Mathematics;

namespace Systems
{
    /// <summary>
    /// Employ unemployed people to employer-tiles with space
    /// Does not handle travelling or anything else
    /// </summary>
    public partial class EmployementSystem : SystemBase
    {
        private EntityQuery employersWithSpaceQuery;
        protected override void OnCreate()
        {
            RequireForUpdate<RunGame>();
            employersWithSpaceQuery = GetEntityQuery(typeof(ActiveTile), typeof(Employer), typeof(HasSpace), typeof(Tile));
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            var entityGrid = SystemAPI.GetBuffer<EntityBufferElement>(SystemAPI.GetSingletonEntity<EntityGridHolder>());

            NativeArray<Entity> employerEntities = employersWithSpaceQuery.ToEntityArray(Allocator.TempJob);
            NativeArray<Employer> employers = employersWithSpaceQuery.ToComponentDataArray<Employer>(Allocator.TempJob);
            NativeArray<Tile> employerMapTileComponents = employersWithSpaceQuery.ToComponentDataArray<Tile>(Allocator.TempJob);
            NativeArray<int> index = new(1, Allocator.TempJob);

            // Employ people
            Entities.WithAll<Unemployed>().ForEach((Entity entity, ref Worker worker, in Person person) =>
            {
                if (index[0] >= employers.Length) return;

                Employer employer = employers[index[0]];
                int2 employerPos = employerMapTileComponents[index[0]].pos;

                if (PathfindingSystem.CalculateTravelTime(person.homeTile, employerPos, entityGrid) == -1) // No valid path to the employer
                    return;

                employer.freeSpace--;
                employers[index[0]] = employer; // Save the updated employer value
                ecb.SetComponent(employerEntities[index[0]], employer); // Save the updated employer value
                worker.employer = employerPos; // Store the employer position

                ecb.RemoveComponent<Unemployed>(entity); // The worker is no longer searching

                if (employer.freeSpace == 0)
                {
                    ecb.RemoveComponent<HasSpace>(employerEntities[index[0]]);
                    index[0]++; // Move to next employer
                }
            }).WithDisposeOnCompletion(employerEntities).WithDisposeOnCompletion(employers).WithDisposeOnCompletion(employerMapTileComponents).WithDisposeOnCompletion(index)
            .Run(); // Must be executed on main thread because PathfindingSystem.CalculateTravelTime() is called (Unity throws errors if Schedule() is used)

            // Remove people from their workplace if there is no valid path
            Entities.WithNone<Unemployed>().ForEach((Entity entity, ref Worker worker, in Person person) =>
            {
                if (PathfindingSystem.CalculateTravelTime(person.homeTile, worker.employer, entityGrid) != -1) // Valid path to employer
                    return;

                // Remove person from the workplace

                // Update employer
                Entity employerEntity = TileGridUtility.GetTile(worker.employer, entityGrid);
                Employer employer = SystemAPI.GetComponent<Employer>(employerEntity);
                employer.freeSpace++;
                ecb.SetComponent(employerEntity, employer);
                if (employer.freeSpace == 1) // If there was no space before
                    ecb.AddComponent<HasSpace>(employerEntity);

                // Make person unemployed
                ecb.AddComponent<Unemployed>(entity);
                ecb.SetComponent(entity, new Worker { employer = -1, timeToWork = -1 });
            }).Run();
        }
    }
}