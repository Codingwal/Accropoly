using Unity.Entities;

public partial class ClearDeactivatedSpaceTilesSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        Entities.WithDisabled<ActiveTileTag>().ForEach((Entity entity, ref Habitat habitat) =>
        {
            habitat.freeSpace = habitat.totalSpace;
            ecb.AddComponent<HasSpaceTag>(entity);
        }).Schedule();

        Entities.WithDisabled<ActiveTileTag>().ForEach((Entity entity, ref Employer employer) =>
        {
            employer.freeSpace = employer.totalSpace;
            ecb.AddComponent<HasSpaceTag>(entity);
        }).Schedule();
    }
}
