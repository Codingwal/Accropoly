using Unity.Entities;

[UpdateInGroup(typeof(ComponentInitializationSystemGroup))]
public partial class SpaceComponentsInitializationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithAll<NewTileTag>().ForEach((Entity entity, ref Habitat habitat) =>
        {
            habitat.freeSpace = habitat.totalSpace;
            EntityManager.AddComponent<HasSpaceTag>(entity);
        }).WithStructuralChanges().Run();
        Entities.WithAll<NewTileTag>().ForEach((Entity entity, ref Employer employer) =>
        {
            employer.freeSpace = employer.totalSpace;
            EntityManager.AddComponent<HasSpaceTag>(entity);
        }).WithStructuralChanges().Run();

        if (SystemAPI.HasSingleton<LoadGameTag>())
        {
            Entities.ForEach((Entity entity, in Habitat habitat) =>
            {
                if (habitat.freeSpace > 0)
                    EntityManager.AddComponent<HasSpaceTag>(entity);
            }).WithStructuralChanges().Run();

            Entities.ForEach((Entity entity, in Employer employer) =>
            {
                if (employer.freeSpace > 0)
                    EntityManager.AddComponent<HasSpaceTag>(entity);
            }).WithStructuralChanges().Run();
        }
    }
}
