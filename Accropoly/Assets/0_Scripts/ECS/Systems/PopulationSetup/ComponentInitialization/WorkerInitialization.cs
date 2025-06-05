using Unity.Entities;
using Components;
using Tags;

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
        protected override void OnCreate()
        {
            RequireForUpdate<Worker>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndComponentInitializationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            Entities.WithAll<NewPerson, Worker>().ForEach((Entity entity) =>
            {
                ecb.SetComponent(entity, new Worker { employer = new(-1, -1) });
                ecb.AddComponent<Unemployed>(entity);
            }).Schedule();

            if (SystemAPI.HasSingleton<LoadGame>())
            {
                Entities.ForEach((Entity entity, in Worker worker) =>
                {
                    if (worker.employer.Equals(new(-1, -1)))
                        ecb.AddComponent<Unemployed>(entity);
                }).Schedule();
            }
        }
    }
}