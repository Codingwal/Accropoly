using Unity.Entities;
using Components;
using Tags;

[UpdateInGroup(typeof(ComponentInitializationSystemGroup))]
public partial class SpaceComponentsInitializationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var ecb = SystemAPI.GetSingleton<EndComponentInitializationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        Entities.WithAll<NewTile>().ForEach((Entity entity, ref Habitat habitat) =>
        {
            habitat.freeSpace = habitat.totalSpace;
            ecb.AddComponent<HasSpace>(entity);
        }).Schedule();
        Entities.WithAll<NewTile>().ForEach((Entity entity, ref Employer employer) =>
        {
            employer.freeSpace = employer.totalSpace;
            ecb.AddComponent<HasSpace>(entity);
        }).Schedule();

        if (SystemAPI.HasSingleton<LoadGame>())
        {
            Entities.ForEach((Entity entity, in Habitat habitat) =>
            {
                if (habitat.freeSpace > 0)
                    ecb.AddComponent<HasSpace>(entity);
            }).Schedule();

            Entities.ForEach((Entity entity, in Employer employer) =>
            {
                if (employer.freeSpace > 0)
                    ecb.AddComponent<HasSpace>(entity);
            }).Schedule();
        }
    }
}
