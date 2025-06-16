using Unity.Entities;

public static class ECSUtility
{
    public static World World => World.DefaultGameObjectInjectionWorld;
    public static EntityManager EntityManager => World.EntityManager;
    public static T GetSingleton<T>() where T : unmanaged, IComponentData
    {
        EntityQueryDesc desc = new()
        {
            All = new ComponentType[] { typeof(T) },
            Options = EntityQueryOptions.IncludeSystems,
        };
        return EntityManager.CreateEntityQuery(desc).GetSingleton<T>();
    }
}
