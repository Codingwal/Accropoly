using Unity.Entities;

[UpdateInGroup(typeof(ComponentInitializationSystemGroup))]
public partial class WorkerInitializationSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<Worker>();
    }
    protected override void OnUpdate()
    {
        var ecb = SystemAPI.GetSingleton<EndComponentInitializationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        Entities.WithAll<NewPersonTag, Worker>().ForEach((Entity entity) =>
        {
            ecb.SetComponent(entity, new Worker { employer = new(-1, -1) });
            ecb.AddComponent<UnemployedTag>(entity);
        }).Schedule();

        if (SystemAPI.HasSingleton<LoadGameTag>())
        {
            Entities.ForEach((Entity entity, in Worker worker) =>
            {
                if (worker.employer.Equals(new(-1, -1)))
                    ecb.AddComponent<UnemployedTag>(entity);
            }).Schedule();
        }
    }
}
