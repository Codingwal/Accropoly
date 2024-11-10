using Unity.Entities;

public partial class ElectricitySystem : SystemBase
{
    private uint frame;
    protected override void OnCreate()
    {
        RequireForUpdate<RunGameTag>();

        // There must be an active consumer or producer for the system to run
        RequireAnyForUpdate(GetEntityQuery(typeof(ElectricityProducer), typeof(ActiveTileTag)), GetEntityQuery(typeof(ElectricityConsumer), typeof(ActiveTileTag)));
    }
    protected override void OnUpdate()
    {
        // Only run this function every 50 frames
        frame++;
        if (frame % 50 != 0) return;

        // Calculate the current production
        float totalProduction = 0;
        Entities.WithAll<ActiveTileTag>().ForEach((in ElectricityProducer producer) =>
        {
            totalProduction += producer.production;
        }).Run();

        // Enable as many consumers as possible
        float totalConsumption = 0;
        float maxConsumption = 0;
        Entities.WithAll<ActiveTileTag>().ForEach((Entity entity, in ElectricityConsumer consumer) =>
        {
            bool canEnable = totalConsumption + consumer.consumption <= totalProduction;
            totalConsumption += canEnable ? consumer.consumption : 0; // Only add to the production if the consumer can be enabled
            EntityManager.SetComponentEnabled<HasElectricityTag>(entity, canEnable);

            maxConsumption += consumer.consumption; // Only for informative purposes
        }).WithoutBurst().Run();

        // Debug.Log($"Electricity: {maxConsumption}/{totalProduction}");
    }
}