using Unity.Entities;
using UnityEngine;

public partial class CheckHabitatExistsSystem : SystemBase
{
    private int frame = 0;
    protected override void OnCreate()
    {
        RequireForUpdate<PersonComponent>();
    }
    protected override void OnUpdate()
    {
        frame++;
        if (frame % 50 != 5) return;

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        Dependency = Entities.WithNone<HomelessTag>().ForEach((Entity entity, ref PersonComponent person) =>
        {
            Entity habitatEntity = TileGridUtility.GetTile(person.homeTile);
            if (!SystemAPI.HasComponent<Habitat>(habitatEntity))
            {
                person.homeTile = new(-1);
                ecb.AddComponent<HomelessTag>(entity);
            }
        }).WithoutBurst().Schedule(Dependency);
    }
}