using Unity.Entities;
using Components;
using Tags;

public partial class ClearDeactivatedHabitatsSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<RunGame>();
    }
    protected override void OnUpdate()
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        Entities.WithDisabled<ActiveTile>().ForEach((Entity entity, ref Habitat habitat) =>
        {
            habitat.freeSpace = habitat.totalSpace;
            ecb.AddComponent<HasSpace>(entity);
        }).Schedule();
    }
}
