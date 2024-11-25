using Components;
using Unity.Entities;
using Tags;

namespace Systems
{
    public partial class UpdateTileActivation : SystemBase
    {
        private int frame;
        protected override void OnCreate()
        {
            RequireForUpdate<RunGame>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            Entities.WithAll<Tile>().ForEach((Entity entity) =>
            {
                ecb.SetComponentEnabled<ActiveTile>(entity, true);
            }).Schedule();

            // Disable buildings without connection (street, ...)
            Entities.WithDisabled<IsConnected>().ForEach((Entity entity) =>
           {
               ecb.SetComponentEnabled<ActiveTile>(entity, false);
           }).Schedule();

            // Disable e-consumers without electricity
            Entities.WithDisabled<HasElectricity>().ForEach((Entity entity, in ElectricityConsumer electricityConsumer) =>
           {
               if (electricityConsumer.disableIfElectroless) // Only disable if electricity is strictly required
                   ecb.SetComponentEnabled<ActiveTile>(entity, false);
           }).Schedule();
        }
    }
}