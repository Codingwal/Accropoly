using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;

public class WorldLoader
{
    private readonly Deserializer deserializer;
    public List<TypeManager.TypeInfo> types;
    private Dictionary<Entity, Entity> entityMap; // old id -> new id
    public WorldLoader(Deserializer _deserializer)
    {
        deserializer = _deserializer;
        types = new();
        entityMap = new();

        foreach (var type in TypeManager.AllTypes)
        {
            if (type.Category == TypeManager.TypeCategory.ComponentData
                && type.Type.GetCustomAttribute(typeof(SaveAttribute)) != null)
            {
                types.Add(type);
            }
        }
    }
    public void Load()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        int count = deserializer.Deserialize<int>();
        for (int i = 0; i < count; i++)
        {
            var name = deserializer.Deserialize<FixedString32Bytes>();

            TypeManager.TypeInfo? type = types.Find((t) => t.Type.Name == name);
            if (type == null)
                throw new($"ComponentType {name} does not exist");

            // this.SaveComponent<type.Type>(entityManager);
            GetType().GetMethod("SaveComponent")
                .MakeGenericMethod(type.Value.Type)
                .Invoke(this, new object[] { entityManager });
        }
    }

    public void LoadComponent<T>(EntityManager entityManager) where T : unmanaged, IComponentData
    {
        int count = deserializer.Deserialize<int>();
        for (int i = 0; i < count; i++)
        {
            Entity entity = deserializer.Deserialize<Entity>();
            T component = deserializer.Deserialize<T>();

            if (!entityMap.ContainsKey(entity))
                entityMap[entity] = entityManager.CreateEntity();

            entityManager.SetComponentData(entityMap[entity], component);
        }
    }
}