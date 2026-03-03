using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;

public class WorldSaver
{
    private readonly Serializer serializer;
    public List<TypeManager.TypeInfo> types;
    public WorldSaver(Serializer _serializer)
    {
        serializer = _serializer;
        types = new();

        foreach (var type in TypeManager.AllTypes)
        {
            if (type.Category == TypeManager.TypeCategory.ComponentData
                && type.Type.GetCustomAttribute(typeof(SaveAttribute)) != null)
            {
                types.Add(type);
            }
        }
    }
    public void Save()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        serializer.Serialize(types.Count);
        foreach (var type in types)
        {
            // this.SaveComponent<type.Type>(entityManager);
            GetType().GetMethod("SaveComponent")
                .MakeGenericMethod(type.Type)
                .Invoke(this, new object[] { entityManager });
        }
    }

    public void SaveComponent<T>(EntityManager entityManager) where T : unmanaged, IComponentData
    {
        EntityQuery query = new EntityQueryBuilder(Allocator.Temp).WithAll<T>().Build(entityManager);

        NativeArray<T> components = query.ToComponentDataArray<T>(Allocator.Temp);
        NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);

        serializer.Serialize<FixedString32Bytes>(nameof(T));
        serializer.Serialize(components.Length);
        for (int i = 0; i < components.Length; i++)
        {
            serializer.Serialize(entities[i]);
            serializer.Serialize(components[i]);
        }
    }
}