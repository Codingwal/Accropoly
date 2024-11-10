using Unity.Entities;
using Unity.Transforms;
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
        Unity.Mathematics.Random rnd = new((uint)Random.Range(1, 1000));

        Dependency = Entities.WithNone<HomelessTag>().ForEach((Entity entity, ref PersonComponent person) =>
        {
            Entity habitatEntity = TileGridUtility.GetTile(person.homeTile);
            if (!SystemAPI.HasComponent<Habitat>(habitatEntity))
            {
                person.homeTile = new(-1);
                ecb.AddComponent<HomelessTag>(entity);
                LocalTransform transform = SystemAPI.GetComponent<LocalTransform>(entity);
                transform.Position = new(-1 + rnd.NextFloat(-0.5f, 0.5f), 0.5f, -1 + rnd.NextFloat(-0.5f, 0.5f));
                ecb.SetComponent(entity, transform);
            }
        }).WithoutBurst().Schedule(Dependency);
    }
}