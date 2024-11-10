using Unity.Entities;

[UpdateInGroup(typeof(ComponentInitializationSystemGroup))]
public partial class ElectricityConsumerInitializationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var ecb = SystemAPI.GetSingleton<EndComponentInitializationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        Entities.WithAll<NewTileTag, ElectricityConsumer>().ForEach((Entity entity) =>
        {
            ecb.AddComponent<HasElectricityTag>(entity);
        }).Schedule();

        if (SystemAPI.HasSingleton<LoadGameTag>())
        {
            Entities.WithAll<ElectricityConsumer>().ForEach((Entity entity) =>
            {
                ecb.AddComponent<HasElectricityTag>(entity);
            }).Schedule();
        }
    }
}
