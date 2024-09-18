using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

public partial struct ElectricitySystem : ISystem
{
    private float totalProduction;
    private float totalConsumption;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RunGameTag>();
    }
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (producer, _) in SystemAPI.Query<RefRO<ElectricityProducer>, NewTileTag>())
            totalProduction += producer.ValueRO.production;

        foreach (var (producer, _) in SystemAPI.Query<RefRO<ElectricityProducer>, NewTileTag>())
            totalProduction += producer.ValueRO.production;
        // Update electricity production
        {
            NativeArray<ElectricityProducer> updatedProducers = SystemAPI.QueryBuilder()
                .WithAll<ElectricityProducer, ActiveTileTag>() // Tile needs to be an active producer
                .WithAny<UpdatedElectricityTag, NewTileTag>().Build() // The tile must have been created or updated (prevents unnecessary calculations)
                .ToComponentDataArray<ElectricityProducer>(Allocator.Temp);
            foreach (var producer in updatedProducers)
            {
                totalProduction += producer.production - producer.previousProduction;
            }
            updatedProducers.Dispose();
        }

        // Update electricity consumption
        {
            NativeArray<ElectricityConsumer> updatedConsumers = SystemAPI.QueryBuilder()
                .WithAll<ElectricityConsumer, ActiveTileTag>() // Tile needs to be an active consumer
                .WithAny<UpdatedElectricityTag, NewTileTag>().Build() // The tile must have been created or updated (prevents unnecessary calculations)
                .ToComponentDataArray<ElectricityConsumer>(Allocator.Temp);
            foreach (var consumer in updatedConsumers)
            {
                totalConsumption += consumer.consumption - consumer.previousConsumption;
            }
            updatedConsumers.Dispose();
        }

        // Update enabled state of consumers
        if (totalConsumption > totalProduction) // If there is a lack of electricity...
        {
            // Disable consumers until there no longer is a lack of electricity
            foreach (var (consumer, _, entity) in SystemAPI.Query<RefRO<ElectricityConsumer>, ActiveTileTag>().WithEntityAccess())
            {
                SystemAPI.SetComponentEnabled<ActiveTileTag>(entity, false); // Disable the building
                totalConsumption -= consumer.ValueRO.consumption;

                if (!(totalConsumption > totalProduction)) break; // If there isn't a lack of electricity anymore, there's nothing more to do
            }
        }
        else if (totalConsumption < totalProduction) // If there is more electricty than required...
        {
            // TODO: This is wrong! (tiles disabled for other reasons will be reactivated by this code)

            NativeArray<ElectricityConsumer> disabledConsumers = SystemAPI.QueryBuilder()
                .WithAll<ElectricityConsumer>().WithNone<ActiveTileTag>().Build() // Tile needs to be an inactive consumer
                .ToComponentDataArray<ElectricityConsumer>(Allocator.Temp);

            foreach (var consumer in )
        }
    }
}