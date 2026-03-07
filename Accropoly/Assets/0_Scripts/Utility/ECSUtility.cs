using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

public static class ECSUtility
{
    public static World World => World.DefaultGameObjectInjectionWorld;
    public static EntityManager EntityManager => World.EntityManager;
    public static T GetSingleton<T>() where T : unmanaged, IComponentData
    {
        return EntityManager.GetComponentData<T>(GetSingletonEntity<T>());
    }
    public static Entity GetSingletonEntity<T>() where T : unmanaged, IComponentData
    {
        EntityQueryDesc desc = new()
        {
            All = new ComponentType[] { typeof(T) },
            Options = EntityQueryOptions.IncludeSystems
        };
        return EntityManager.CreateEntityQuery(desc).GetSingletonEntity(); 
    }
    public static RefRW<T> GetSingletonRW<T>() where T : unmanaged, IComponentData
    {
        EntityQueryDesc desc = new()
        {
            All = new ComponentType[] { typeof(T) },
            Options = EntityQueryOptions.IncludeSystems
        };
        return EntityManager.CreateEntityQuery(desc).GetSingletonRW<T>();
    }
    public static bool TryGetSingleton<T>(out T value) where T : unmanaged, IComponentData
    {
        EntityQueryDesc desc = new()
        {
            All = new ComponentType[] { typeof(T) },
            Options = EntityQueryOptions.IncludeSystems
        };
        return EntityManager.CreateEntityQuery(desc).TryGetSingleton(out value);
    }
}
public unsafe struct Ref<T> where T : unmanaged
{
    [NativeDisableUnsafePtrRestriction]
    private readonly T* data;
    public Ref(ref T data)
    {
        this.data = (T*)UnsafeUtility.AddressOf(ref data);
    }
    public Ref(T* data)
    {
        this.data = data;
    }
    public readonly bool IsValid => data != null;
    public ref T ValueRW
    {
        get
        {
            return ref UnsafeUtility.AsRef<T>(data);
        }
    }
    public readonly ref readonly T ValueRO
    {
        get
        {
            return ref UnsafeUtility.AsRef<T>(data);
        }
    }
}

public struct CopyComponent<T> : IComponentData where T : struct, IComponentData
{
    public T value;
    public CopyComponent(T value)
    {
        this.value = value;
    }
}