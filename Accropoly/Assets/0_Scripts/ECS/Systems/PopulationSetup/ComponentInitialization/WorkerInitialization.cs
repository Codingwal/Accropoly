using Unity.Entities;
using Components;
using Tags;
using Unity.Collections;

namespace Systems
{
    /// <summary>
    /// Re-add unemployed tag to unemployed people after world loading
    /// (The tag isn't stored in the save file to reduce size)
    /// Also add the tag to new people (they are obviously unemployed) and set their employer to (-1, -1)
    /// </summary>
    [UpdateInGroup(typeof(ComponentInitializationSystemGroup))]
    public partial class WorkerInitialization : SystemBase
    {
        private EntityQuery newWorkers;
        protected override void OnCreate()
        {
            RequireForUpdate<Worker>();
            newWorkers = new EntityQueryBuilder(Allocator.Temp).WithAll<NewPerson, Worker>().Build(this);
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndComponentInitializationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            ecb.AddComponent(newWorkers, new Worker { employer = new(-1, -1) }); // Set not add, but set doesn't have the required overload
            ecb.AddComponent<Unemployed>(newWorkers, EntityQueryCaptureMode.AtPlayback);

            if (SystemAPI.HasSingleton<LoadGame>())
            {
                foreach (var (worker, entity) in SystemAPI.Query<RefRO<Worker>>().WithEntityAccess())
                {
                    if (worker.ValueRO.employer.Equals(new(-1, -1)))
                        ecb.AddComponent<Unemployed>(entity);
                }
            }
        }
    }
}