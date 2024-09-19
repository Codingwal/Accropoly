using Unity.Entities;
using UnityEngine;

public partial class ElectricitySystem : SystemBase
{
    private float totalProduction;
    private float totalConsumption;
    protected override void OnCreate()
    {
        RequireForUpdate<RunGameTag>();
    }
    protected override void OnUpdate()
    {
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

                if (!(totalConsumption > totalProduction)) enoughElectricity = true;
            }).WithoutBurst().Run();
        }
        else if (totalConsumption < totalProduction) // If there is more electricity than required...
        {
            Entities.WithNone<HasElectricityTag>().ForEach((Entity entity, in ElectricityConsumer consumer) =>
            {
                if (totalConsumption + consumer.consumption < totalProduction) // If it can be enabled without creating a lack of enelectricity
                {
                    totalConsumption += consumer.consumption;
                    SystemAPI.SetComponentEnabled<HasElectricityTag>(entity, true); // Reenable the tile
                }
            }).WithoutBurst().Run();
        }
    }
}