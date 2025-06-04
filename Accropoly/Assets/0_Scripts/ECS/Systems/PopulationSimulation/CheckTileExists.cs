using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Components;
using Tags;

namespace Systems
{
    public partial class CheckTileExists : SystemBase
    {
        private EntityQuery newTilesQuery;
        private EntityQuery disabledTilesQuery;
        protected override void OnCreate()
        {
            RequireForUpdate<RunGame>();

            newTilesQuery = GetEntityQuery(typeof(NewTile));
            disabledTilesQuery = GetEntityQuery(new EntityQueryDesc { Disabled = new ComponentType[] { typeof(ActiveTile) }, All = new ComponentType[] { typeof(Tile) } });
        }
        protected override void OnUpdate()
        {
            int newTilesCount = newTilesQuery.CalculateEntityCount();
            int disabledTilesCount = disabledTilesQuery.CalculateEntityCount();

            if (newTilesCount == 0 && disabledTilesCount == 0) return;

            // Get all positions of deleted (replaced) tiles
            NativeArray<int2> newTilesPositions = new(newTilesCount, Allocator.TempJob);
            var inputDeps = Entities.WithAll<NewTile>().ForEach((int entityInQueryIndex, in Tile mapTileComponent) =>
            {
                newTilesPositions[entityInQueryIndex] = mapTileComponent.pos;
            }).Schedule(Dependency);

            // Get all positions of disabled tiles
            NativeArray<int2> disabledTilesPositions = new(disabledTilesCount, Allocator.TempJob);
            inputDeps = Entities.WithDisabled<ActiveTile>().ForEach((int entityInQueryIndex, in Tile mapTileComponent) =>
            {
                disabledTilesPositions[entityInQueryIndex] = mapTileComponent.pos;
            }).Schedule(inputDeps);


            // Make people homeless if their home is deactivated / has been replaced
            var ecb1 = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            Random rnd = new((uint)UnityEngine.Random.Range(1, 1000));
            JobHandle handle1 = Entities.WithNone<Homeless>().ForEach((Entity entity, ref Person person) =>
            {
                int2 homeTilePos = person.homeTile;
                if (newTilesPositions.Contains(homeTilePos) || disabledTilesPositions.Contains(homeTilePos))
                {
                    person.homeTile = new(-1);
                    ecb1.AddComponent<Homeless>(entity);

                    // Homeless people are collected at a special position
                    LocalTransform transform = SystemAPI.GetComponent<LocalTransform>(entity);
                    transform.Position = new(-1 + rnd.NextFloat(-0.5f, 0.5f), 0.5f, -1 + rnd.NextFloat(-0.5f, 0.5f));
                    ecb1.SetComponent(entity, transform);
                }
            }).WithReadOnly(newTilesPositions).WithReadOnly(disabledTilesPositions).Schedule(inputDeps);

            // Make people unemployed if their employer is deactivated / has been replaced
            var ecb2 = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            JobHandle handle2 = Entities.WithNone<Unemployed>().ForEach((Entity entity, ref Worker worker) =>
            {
                int2 employerPos = worker.employer;
                if (newTilesPositions.Contains(employerPos))
                {
                    worker.employer = new(-1);
                    ecb2.AddComponent<Unemployed>(entity);
                }
            }).WithReadOnly(newTilesPositions).Schedule(inputDeps);

            // Dispose NativeArrays after all jobs that use them have been completed
            Dependency = newTilesPositions.Dispose(JobHandle.CombineDependencies(handle1, handle2));
            Dependency = disabledTilesPositions.Dispose(JobHandle.CombineDependencies(handle1, handle2));
        }
    }
}