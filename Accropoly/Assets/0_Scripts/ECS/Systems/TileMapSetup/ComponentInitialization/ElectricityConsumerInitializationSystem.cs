using Unity.Entities;

public partial class ElectricityConsumerInitializationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithAll<NewTileTag, ElectricityConsumer>().ForEach((Entity entity) =>
        {
            EntityManager.AddComponent<HasElectricityTag>(entity);
        }).WithoutBurst().WithStructuralChanges().Run();
    }
}
