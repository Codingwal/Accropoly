using Unity.Entities;

public partial class ClearDeactivatedHabitatsSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<RunGameTag>();
    }
    protected override void OnUpdate()
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        Entities.WithDisabled<ActiveTileTag>().ForEach((Entity entity, ref Habitat habitat) =>
        {
            habitat.freeSpace = habitat.totalSpace;
            ecb.AddComponent<HasSpaceTag>(entity);
        }).Schedule();
    }
}
