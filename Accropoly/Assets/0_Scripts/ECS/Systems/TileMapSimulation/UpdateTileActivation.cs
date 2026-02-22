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
            RequireForUpdate<RunGame>();
        }
        protected override void OnUpdate()
        {
            // Enable all tiles by deault
            EntityManager.SetComponentEnabled<ActiveTile>(SystemAPI.QueryBuilder().WithPresent<ActiveTile>().Build(), true);

            // Disable buildings without connection (street, ...)
            EntityManager.SetComponentEnabled<ActiveTile>(SystemAPI.QueryBuilder().WithPresent<ActiveTile>().WithDisabled<IsConnected>().Build(), false);

            // Disable e-consumers without electricity (if electircity is required)
            foreach (var (consumer, entity) in SystemAPI.Query<RefRO<ElectricityConsumer>>().WithEntityAccess())
            {
                if (consumer.ValueRO.disableIfElectroless) // Only disable if electricity is strictly required
                    SystemAPI.SetComponentEnabled<ActiveTile>(entity, false);
            }
        }
    }
}