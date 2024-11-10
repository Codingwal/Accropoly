using Unity.Entities;
using UnityEngine;

public partial class CheckEmployerExistsSystem : SystemBase
{
    private int frame;
    protected override void OnCreate()
    {
        RequireForUpdate<Worker>();
    }
    protected override void OnUpdate()
    {
        frame++;
        if (frame % 50 != 6) return;

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        Entities.WithNone<UnemployedTag>().ForEach((Entity entity, ref Worker worker) =>
        {
            Entity employerEntity = TileGridUtility.GetTile(worker.employer);
            if (!SystemAPI.HasComponent<Employer>(employerEntity))
            {
                worker.employer = new(-1);
                ecb.AddComponent<UnemployedTag>(entity);
            }
        }).WithoutBurst().Schedule();
    }
}
