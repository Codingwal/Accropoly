using Unity.Entities;
using UnityEngine;

public partial class ElectricitySystem : SystemBase
{
    private uint frame;
    protected override void OnCreate()
    {
        RequireForUpdate<RunGameTag>();
    }
    protected override void OnUpdate()
    {
        // Only run this function every 10 frames
        if (frame < 10)
        {
            frame++;
            return;
        }
        frame = 0;

        // Calculate the current production
        float totalProduction = 0;
        Entities.WithAll<ActiveTileTag>().ForEach((in ElectricityProducer producer) =>
        {
            totalProduction += producer.production;
        }).Run();

        // Enable as many consumers as possible
        float totalConsumption = 0;
        Entities.WithAll<ActiveTileTag>().ForEach((Entity entity, in ElectricityConsumer consumer) =>
        {
            bool canEnable = totalConsumption + consumer.consumption <= totalProduction;
            totalConsumption += canEnable ? consumer.consumption : 0; // Only add to the production if the consumer can be enabled
        }).Run();
            EntityManager.SetComponentEnabled<HasElectricityTag>(entity, canEnable);

        Debug.Log($"{totalConsumption}/{totalProduction}");
    }
}

// More efficient but also more complex electricty system. Disabled tiles are not handled correctly.
/*
Debug.Log($"{totalConsumption}/{totalProduction}");
Entities.WithAll<ActiveTileTag>().WithAny<NewTileTag, UpdatedElectricityTag>().ForEach((in ElectricityProducer producer) =>
{
    totalProduction += producer.production - producer.previousProduction;
}).WithoutBurst().Run();

Entities.WithAll<ActiveTileTag>().WithAny<NewTileTag, UpdatedElectricityTag>().ForEach((in ElectricityConsumer consumer) =>
{
    totalConsumption += consumer.consumption - consumer.previousConsumption;
}).WithoutBurst().Run();

// Update enabled state of consumers
if (totalConsumption > totalProduction) // If there is a lack of electricity...
{
    // Disable consumers until there no longer is a lack of electricity
    bool enoughElectricity = false;
    Entities.WithAll<ActiveTileTag>().ForEach((Entity entity, in ElectricityConsumer consumer) =>
    {
        if (enoughElectricity) return; // If there isn't a lack of electricity anymore, there's nothing more to do

        SystemAPI.SetComponentEnabled<HasElectricityTag>(entity, false); // Disable the tile
        totalConsumption -= consumer.consumption;

        if (totalConsumption <= totalProduction) enoughElectricity = true;
    }).WithoutBurst().Run();
}
else if (totalConsumption < totalProduction) // If there is more electricity than required...
{
    Entities.WithNone<HasElectricityTag>().ForEach((Entity entity, in ElectricityConsumer consumer) =>
    {
        if (totalConsumption + consumer.consumption < totalProduction) // If it can be enabled without creating a lack of electricity
        {
            totalConsumption += consumer.consumption;
            SystemAPI.SetComponentEnabled<HasElectricityTag>(entity, true); // Reenable the tile
        }
    }).WithoutBurst().Run();
}
*/