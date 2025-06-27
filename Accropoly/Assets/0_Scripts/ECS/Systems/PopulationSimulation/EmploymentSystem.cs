using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Components;
using Tags;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

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
            var gameInfo = SystemAPI.GetSingleton<GameInfo>();
            if (!gameInfo.time.NewHour) return;

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            var entityGrid = TileGridUtility.GetEntityGrid();

            NativeList<Entity> employerEntities = employersWithSpaceQuery.ToEntityListAsync(Allocator.TempJob, out var handle);
            handle.Complete();

            // Employ people
            Entities.WithAll<Unemployed>().ForEach((Entity entity, ref Worker worker, in Person person) =>
            {
                if (employerEntities.IsEmpty)
                    return;

                for (int i = 0; i < employerEntities.Length; i++)
                {
                    Entity employerEntity = employerEntities[i];
                    Employer employer = SystemAPI.GetComponent<Employer>(employerEntity);
                    int2 employerPos = SystemAPI.GetComponent<Tile>(employerEntity).pos;

                    if (employer.freeSpace == 0) // HasSpace tag might still be present because the employer was filled in this frame
                        Debug.LogError("!");

                    if (PathfindingSystem.CalculateTravelTime(person.homeTile, employerPos, entityGrid) == -1) // No valid path to the employer
                        continue;

                    // Update employer
                    employer.freeSpace--;
                    SystemAPI.SetComponent(employerEntity, employer); // Can't use ecb because the field might be updated multiple times
                    if (employer.freeSpace == 0)
                    {
                        ecb.RemoveComponent<HasSpace>(employerEntity);
                        employerEntities.RemoveAtSwapBack(i); // Following unemployed people should not check full employers
                    }

                    // Update person
                    worker.employer = employerPos;
                    ecb.RemoveComponent<Unemployed>(entity);

                    break; // Stop searching for an employer if a valid employer has been found
                }
            }).WithDisposeOnCompletion(employerEntities)
            .WithoutBurst().Run(); // Must be executed on main thread because PathfindingSystem.CalculateTravelTime() is called (Unity throws errors if Schedule() is used)

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
                SystemAPI.SetComponent(employerEntity, employer); // Can't use ecb because the field might be updated multiple times
                if (employer.freeSpace == 1) // If there was no space before
                    ecb.AddComponent<HasSpace>(employerEntity);

                // Make person unemployed
                ecb.AddComponent<Unemployed>(entity);
                ecb.SetComponent(entity, new Worker { employer = -1, timeToWork = -1 });
            }).WithoutBurst().Run(); // Must be executed on main thread because PathfindingSystem.CalculateTravelTime() is called (Unity throws errors if Schedule() is used)
        }
    }
}