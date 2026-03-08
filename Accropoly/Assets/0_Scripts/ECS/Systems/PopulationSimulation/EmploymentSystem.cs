using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Components;
using Tags;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using Components.WaypointComponents;
using Unity.Burst;

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
            employersWithSpaceQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<ActiveTile, Employer, HasSpace, Tile>().Build(this);
            RequireForUpdate<WaypointsData>();
        }
        protected override void OnUpdate()
        {
            var gameInfo = SystemAPI.GetSingleton<GameInfo>();
            if (!gameInfo.time.NewHour) return;

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            var entityGrid = TileGridUtility.GetEntityGrid();

            NativeList<Entity> employerEntities = employersWithSpaceQuery.ToEntityListAsync(Allocator.TempJob, out var handle);
            handle.Complete();

            var pathfindingUtility = new PathfindingUtility()
            {
                transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(isReadOnly: true),
                connectionsLookup = SystemAPI.GetBufferLookup<Connection>(isReadOnly: true),
                waypointLookup = SystemAPI.GetComponentLookup<Waypoint>(isReadOnly: true),
                waypointsData = SystemAPI.GetSingleton<WaypointsData>(),
            };

            // Employ people
            new EmployPeopleJob
            {
                ecb = ecb,
                pathfindingUtility = pathfindingUtility,
                employerEntities = employerEntities,
                employerLookup = SystemAPI.GetComponentLookup<Employer>(),
                tileLookup = SystemAPI.GetComponentLookup<Tile>(isReadOnly: true),
            }.Schedule();

            // Remove people from their workplace if there is no valid path
            new UnemployIfNoPathJob
            {
                ecb = ecb,
                pathfindingUtility = pathfindingUtility,
                entityGrid = entityGrid,
                employerLookup = SystemAPI.GetComponentLookup<Employer>(),
            }.Schedule();

            employerEntities.Dispose(Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(Unemployed))]
        private partial struct EmployPeopleJob : IJobEntity
        {
            public EntityCommandBuffer ecb;
            public PathfindingUtility pathfindingUtility;
            public NativeList<Entity> employerEntities;
            public ComponentLookup<Employer> employerLookup;
            [ReadOnly] public ComponentLookup<Tile> tileLookup;
            public void Execute(Entity entity, ref Worker worker, in Person person)
            {
                if (employerEntities.IsEmpty)
                    return;

                for (int i = 0; i < employerEntities.Length; i++)
                {
                    Entity employerEntity = employerEntities[i];
                    Employer employer = employerLookup[employerEntity];
                    int2 employerPos = tileLookup[employerEntity].pos;

                    if (employer.freeSpace == 0) // HasSpace tag might still be present because the employer was filled in this frame
                        Debug.LogError("!");

                    if (pathfindingUtility.CalculateTravelTime(person.homeTile, employerPos) == -1) // No valid path to the employer
                        continue;

                    // Update employer
                    employer.freeSpace--;
                    employerLookup[employerEntity] = employer; // Can't use ecb because the field might be updated multiple times
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
            }
        }

        [BurstCompile]
        [WithNone(typeof(Unemployed))]
        private partial struct UnemployIfNoPathJob : IJobEntity
        {
            public EntityCommandBuffer ecb;
            public PathfindingUtility pathfindingUtility;
            public DynamicBuffer<EntityBufferElement> entityGrid;
            public ComponentLookup<Employer> employerLookup;
            public void Execute(Entity entity, ref Worker worker, in Person person)
            {
                if (pathfindingUtility.CalculateTravelTime(person.homeTile, worker.employer) != -1) // Valid path to employer
                    return;

                // Remove person from the workplace

                // Update employer
                Entity employerEntity = TileGridUtility.GetTile(worker.employer, entityGrid);
                Employer employer = employerLookup[employerEntity];
                employer.freeSpace++;
                employerLookup[employerEntity] = employer;
                if (employer.freeSpace == 1) // If there was no space before
                    ecb.AddComponent<HasSpace>(employerEntity);

                // Make person unemployed
                ecb.AddComponent<Unemployed>(entity);
                ecb.SetComponent(entity, new Worker { employer = -1, timeToWork = -1 });
            }
        }
    }
}