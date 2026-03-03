using System.Reflection;
using Unity.Collections;
using Unity.Entities;

public struct ComponentSaver
{
    private readonly Serializer serializer;
    public ComponentSaver(Serializer _serializer)
    {
        serializer = _serializer;
    }
    public readonly void Save()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        foreach (var type in TypeManager.AllTypes)
        {
            if (type.Category != TypeManager.TypeCategory.ComponentData)
                continue;

            if (type.Type.GetCustomAttribute(typeof(SaveAttribute)) == null)
                continue;

            // this.SaveComponent<type.Type>(entityManager);
            typeof(Serializer).GetMethod("SaveComponent")
                .MakeGenericMethod(type.Type)
                .Invoke(this, new object[] { entityManager });
        }
    }

    public readonly void SaveComponent<T>(EntityManager entityManager) where T : unmanaged, IComponentData
    {
        EntityQuery query = new EntityQueryBuilder(Allocator.Temp).WithAll<T>().Build(entityManager);

        NativeArray<T> components = query.ToComponentDataArray<T>(Allocator.Temp);
        NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);

        serializer.Serialize(components.Length);
        for (int i = 0; i < components.Length; i++)
        {
            serializer.Serialize(entities[i]);
            serializer.Serialize(components[i]);
        }
    }
}