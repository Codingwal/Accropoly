using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Components;
using Tags;

namespace Systems
{
    /// <summary>
    /// Calculate electricity production and enable as many buildings as possible
    /// Also update UIInfo with electricity related information (used for statistics display)
    /// </summary>
    public partial class ElectricitySystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<RunGame>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            // Calculate the current production
            float totalProduction = 0;
            foreach (var producer in SystemAPI.Query<RefRO<ElectricityProducer>>().WithAll<ActiveTile>())
            {
                totalProduction += producer.ValueRO.production;
            }

            // Enable as many consumers as possible
            // Tiles with & without electricity are split so that the distribution is consistent across frames

            float totalConsumption = 0f;
            float maxConsumption = 0f;

            void UpdateEntity(Entity entity, in ElectricityConsumer consumer)
            {
                bool canEnable = totalConsumption + consumer.consumption <= totalProduction;
                totalConsumption += canEnable ? consumer.consumption : 0; // Only add to the production if the consumer can be enabled
                ecb.SetComponentEnabled<HasElectricity>(entity, canEnable);

                maxConsumption += consumer.consumption; // Only for informative purposes
            }

            foreach (var (consumer, entity) in SystemAPI.Query<RefRO<ElectricityConsumer>>()
                .WithAll<HasElectricity>().WithNone<DisabledTile>().WithEntityAccess())
            {
                UpdateEntity(entity, consumer.ValueRO);
            }

            foreach (var (consumer, entity) in SystemAPI.Query<RefRO<ElectricityConsumer>>()
                .WithDisabled<HasElectricity>().WithNone<DisabledTile>().WithEntityAccess())
            {
                UpdateEntity(entity, consumer.ValueRO);
            }

            // Update UIInfo (used for the statistics display)
            var info = SystemAPI.GetSingletonRW<UIInfo>();
            info.ValueRW.electricityProduction = totalProduction;
            info.ValueRW.actualElectricityConsumption = totalConsumption;
            info.ValueRW.maxElectricityConsumption = maxConsumption;
        }
    }
}