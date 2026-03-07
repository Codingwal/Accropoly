using Components;
using Unity.Entities;
using Tags;

namespace Systems
{
    /// <summary>
    /// Update which tiles are active
    /// Deactivation reasons: 
    /// - no connection (to street for example)
    /// - no electricity (if strictly required)
    /// </summary>
    public partial class UpdateTileActivation : SystemBase
    {
        protected override void OnCreate()
        {
            
        }
        protected override void OnUpdate()
        {
            // Enable all tiles by default
            EntityManager.SetComponentEnabled<ActiveTile>(SystemAPI.QueryBuilder().WithPresent<ActiveTile>().Build(), true);

            EntityQuery unconnectedTiles = SystemAPI.QueryBuilder().WithAll<ActiveTile>().WithDisabled<IsConnected>().Build();

            // Disable buildings without connection (street, ...)
            // I tried doing this using the EntityManager, but that didn't work somehow
            foreach (var entity in unconnectedTiles.ToEntityArray(Unity.Collections.Allocator.Temp))
            {
                SystemAPI.SetComponentEnabled<ActiveTile>(entity, false);
            }

            // Disable e-consumers without electricity (if electricity is required)
            foreach (var (consumer, entity) in SystemAPI.Query<RefRO<ElectricityConsumer>>().WithDisabled<HasElectricity>().WithEntityAccess())
            {
                if (consumer.ValueRO.disableIfElectroless) // Only disable if electricity is strictly required
                    SystemAPI.SetComponentEnabled<ActiveTile>(entity, false);
            }
        }
    }
}