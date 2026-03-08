using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Components;
using Tags;
using Unity.Burst;

namespace Systems
{
    /// <summary>
    /// Make people homeless/unemployed if their house/employer is removed/inactive
    /// Also update the tiles free space
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)] // Related updates using ref values / ECBs should be synchronous (No system should execute between them)
    public partial class CheckTileExists : SystemBase
    {
        private EntityQuery newTilesQuery;
        private EntityQuery disabledTilesQuery;
        protected override void OnCreate()
        {
            newTilesQuery = GetEntityQuery(typeof(NewTile));
            disabledTilesQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Tile>().WithDisabled<ActiveTile>().Build(this);
        }
        protected override void OnUpdate()
        {
            int newTilesCount = newTilesQuery.CalculateEntityCount();
            int disabledTilesCount = disabledTilesQuery.CalculateEntityCount();

            if (newTilesCount == 0 && disabledTilesCount == 0) return;

            NativeList<int2> tiles = new(newTilesCount + disabledTilesCount, Allocator.TempJob);

            // Get all positions of deleted (replaced) tiles
            foreach (var tile in SystemAPI.Query<RefRO<Tile>>().WithAll<NewTile>())
            {
                tiles.Add(tile.ValueRO.pos);
            }

            // Get all positions of disabled tiles
            foreach (var tile in SystemAPI.Query<RefRO<Tile>>().WithDisabled<ActiveTile>())
            {
                tiles.Add(tile.ValueRO.pos);
            }

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            Random rnd = new((uint)UnityEngine.Random.Range(1, 1000));

            // Make people homeless if their home is deactivated / has been replaced
            new MakeHomelessJob
            {
                transformLookup = GetComponentLookup<LocalTransform>(),
                tiles = tiles,
                ecb = ecb,
                rnd = rnd,
            }.Schedule();

            // Make people unemployed if their employer is deactivated / has been replaced
            new MakeUnemployedJob
            {
                tiles = tiles,
                ecb = ecb,
            }.Schedule();

            tiles.Dispose(Dependency);
        }

        [BurstCompile]
        private partial struct MakeHomelessJob : IJobEntity 
        {
            [ReadOnly] public ComponentLookup<LocalTransform> transformLookup;
            public NativeList<int2> tiles;
            public EntityCommandBuffer ecb;
            public Random rnd;
            public void Execute(Entity entity, ref Person person)
            {
                int2 homeTilePos = person.homeTile;
                if (tiles.Contains(homeTilePos))
                {
                    // These changes need to be synchronous -> Reason why the system executes directly before the ECBS
                    person.homeTile = new(-1);
                    ecb.AddComponent<Homeless>(entity);

                    // Homeless people are collected at a special position
                    LocalTransform transform = transformLookup[entity];
                    transform.Position = new(-1 + rnd.NextFloat(-0.5f, 0.5f), 0.5f, -1 + rnd.NextFloat(-0.5f, 0.5f));
                    transformLookup[entity] = transform;
                }
            }
        }

        [BurstCompile]
        private partial struct MakeUnemployedJob : IJobEntity
        {
            [ReadOnly] public NativeList<int2> tiles;
            public EntityCommandBuffer ecb;
            public void Execute(Entity entity, ref Worker worker)
            {
                int2 employerPos = worker.employer;
                if (tiles.Contains(employerPos))
                {
                    worker.employer = new(-1);
                    ecb.AddComponent<Unemployed>(entity);
                }
            }
        }
    }
}