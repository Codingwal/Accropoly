using Unity.Entities;

[UpdateInGroup(typeof(ComponentInitializationSystemGroup))]
public partial class ElectricityConsumerInitializationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var ecb = SystemAPI.GetSingleton<EndCreationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        Entities.WithAll<NewTileTag, ElectricityConsumer>().ForEach((Entity entity) =>
        {
            ecb.AddComponent<HasElectricityTag>(entity);
        }).Run();

        if (SystemAPI.HasSingleton<LoadGameTag>())
        {
            Entities.WithAll<ElectricityConsumer>().ForEach((Entity entity) =>
            {
                ecb.AddComponent<HasElectricityTag>(entity);
            }).Run();
        }
    }
}
