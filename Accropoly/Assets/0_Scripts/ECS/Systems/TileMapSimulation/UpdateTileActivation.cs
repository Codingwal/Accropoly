using Components;
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

        Entities.WithAll<Tile>().ForEach((Entity entity) =>
        {
            ecb.SetComponentEnabled<ActiveTileTag>(entity, true);
        }).Schedule();

        // Disable buildings without connection (street, ...)
        Entities.WithDisabled<IsConnectedTag>().ForEach((Entity entity) =>
       {
           ecb.SetComponentEnabled<ActiveTileTag>(entity, false);
       }).Schedule();

        // Disable e-consumers without electricity
        Entities.WithDisabled<HasElectricityTag>().ForEach((Entity entity, in ElectricityConsumer electricityConsumer) =>
       {
           if (electricityConsumer.disableIfElectroless) // Only disable if electricity is strictly required
               ecb.SetComponentEnabled<ActiveTileTag>(entity, false);
       }).Schedule();
    }
}
