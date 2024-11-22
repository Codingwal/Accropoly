using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Components;
using Tags;
using NUnit.Framework;

namespace Systems
{
    public partial class ElectricitySystem : SystemBase
    {
        private uint frame;
        protected override void OnCreate()
        {
            RequireForUpdate<RunGame>();
        }
        protected override void OnUpdate()
        {
            // Only run this function every 50 frames
            frame++;
            if (frame % 50 != 0) return;

            // Calculate the current production
            NativeArray<float> totalProduction = new(1, Allocator.TempJob, NativeArrayOptions.ClearMemory);
            Entities.WithAll<ActiveTile>().ForEach((in ElectricityProducer producer) =>
            {
                totalProduction[0] += producer.production;
            }).Schedule();

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            // Enable as many consumers as possible
            // Tiles with & without electrcity are split so that the distribution is consistent across frames
            NativeArray<float> totalConsumption = new(1, Allocator.TempJob, NativeArrayOptions.ClearMemory);
            NativeArray<float> maxConsumption = new(1, Allocator.TempJob, NativeArrayOptions.ClearMemory);
            Entities.WithAll<ActiveTile, HasElectricity>().ForEach((Entity entity, in ElectricityConsumer consumer) =>
            {
                bool canEnable = totalConsumption[0] + consumer.consumption <= totalProduction[0];
                totalConsumption[0] += canEnable ? consumer.consumption : 0; // Only add to the production if the consumer can be enabled
                ecb.SetComponentEnabled<HasElectricity>(entity, canEnable);

                maxConsumption[0] += consumer.consumption; // Only for informative purposes
            }).Schedule();
            Entities.WithAll<ActiveTile>().WithDisabled<HasElectricity>().ForEach((Entity entity, in ElectricityConsumer consumer) =>
            {
                bool canEnable = totalConsumption[0] + consumer.consumption <= totalProduction[0];
                totalConsumption[0] += canEnable ? consumer.consumption : 0; // Only add to the production if the consumer can be enabled
                ecb.SetComponentEnabled<HasElectricity>(entity, canEnable);

                maxConsumption[0] += consumer.consumption; // Only for informative purposes
            }).Schedule();

            // Update UIInfo (used for the statistics display)
            Entities.ForEach((ref UIInfo info) =>
            {
                info.electricityProduction = totalProduction[0];
                info.actualElectricityConsumption = totalConsumption[0];
                info.maxElectricityConsumption = maxConsumption[0];
            }).WithDisposeOnCompletion(totalProduction).WithDisposeOnCompletion(totalConsumption).WithDisposeOnCompletion(maxConsumption).Schedule();
        }
    }
}