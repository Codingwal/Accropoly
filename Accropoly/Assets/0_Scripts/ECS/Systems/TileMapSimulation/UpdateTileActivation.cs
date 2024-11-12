using Unity.Entities;

public partial class UpdateTileActivation : SystemBase
{
    private int frame;
    protected override void OnCreate()
    {
        RequireForUpdate<RunGameTag>();
    }
    protected override void OnUpdate()
    {
        frame++;
        if (frame % 50 != 0) return;

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        Dependency = Entities.WithAll<MapTileComponent>().ForEach((Entity entity) =>
        {
            ecb.SetComponentEnabled<ActiveTileTag>(entity, true);
        }).Schedule(Dependency);

        // Disable buildings without connection (street, ...)
        Dependency = Entities.WithDisabled<IsConnectedTag>().ForEach((Entity entity) =>
        {
            ecb.SetComponentEnabled<ActiveTileTag>(entity, false);
        }).Schedule(Dependency);

        // Disable e-consumers without electricity
        Dependency = Entities.WithDisabled<HasElectricityTag>().ForEach((Entity entity, in ElectricityConsumer electricityConsumer) =>
        {
            if (electricityConsumer.disableIfElectroless) // Only disable if electricity is strictly required
                ecb.SetComponentEnabled<ActiveTileTag>(entity, false);
        }).Schedule(Dependency);

        // Disable employers without at least one employee
        // Dependency = Entities.ForEach((Entity entity, in Employer employer) =>
        // {
        //     if (employer.freeSpace == employer.totalSpace) // If the employer doesn't have employees
        //         ecb.SetComponentEnabled<ActiveTileTag>(entity, false);
        // }).Schedule(Dependency);
    }
}
