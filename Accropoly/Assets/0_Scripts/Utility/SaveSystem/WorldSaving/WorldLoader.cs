using System.Collections.Generic;
using System.Reflection;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

public class WorldLoader
{
    public List<TypeManager.TypeInfo> types;
    private Dictionary<int, Entity> entityMap; // old id -> new id
    private EntityManager entityManager;
    public WorldLoader(EntityManager _entityManager)
    {
        entityMap = new();
        entityManager = _entityManager;

        types = SaveComponents.GetSaveComponentTypeInfos();
    }
    public void Load(WorldSave save)
    {
        foreach (var componentSave in save.componentSaves)
        {
            TypeManager.TypeInfo typeInfo = GetTypeInfo(componentSave.name.ToString());

            if (typeInfo.Category == TypeManager.TypeCategory.ComponentData)
            {
                // this.LoadComponent<type.Type>(componentSave);
                MethodInfo method = GetType().GetMethod(nameof(LoadComponent), BindingFlags.NonPublic | BindingFlags.Instance);
                method = method.MakeGenericMethod(typeInfo.Type);
                method.Invoke(this, new object[] { componentSave });
            }
            else if (typeInfo.Category == TypeManager.TypeCategory.BufferData)
            {
                // this.LoadBuffer<type.Type>(componentSave);
                MethodInfo method = GetType().GetMethod(nameof(LoadBuffer), BindingFlags.NonPublic | BindingFlags.Instance);
                method = method.MakeGenericMethod(typeInfo.Type);
                method.Invoke(this, new object[] { componentSave });
            }
            else
                Debug.Log($"Can't deserialize type {typeInfo.Type}");
        }
    }

    private void LoadBuffer<T>(WorldSave.ComponentSave save)
        where T : unmanaged, IBufferElementData
    {
        Deserializer componentDeserializer = new(new NativeListReader(save.components));

        for (int i = 0; i < save.entityIds.Length; i++)
        {
            int entityId = save.entityIds[i];

            if (!entityMap.ContainsKey(entityId))
                entityMap[entityId] = entityManager.CreateEntity();

            var buffer = entityManager.AddBuffer<T>(entityMap[entityId]);

            int length = componentDeserializer.Deserialize<int>();
            for (int j = 0; j < length; j++)
            {
                T element = componentDeserializer.Deserialize<T>();
                buffer.Add(element);
            }
        }
    }

    private void LoadComponent<T>(WorldSave.ComponentSave save)
        where T : unmanaged, IComponentData
    {
        Deserializer componentDeserializer = new(new NativeListReader(save.components));

        for (int i = 0; i < save.entityIds.Length; i++)
        {
            int entityId = save.entityIds[i];

            if (!entityMap.ContainsKey(entityId))
                entityMap[entityId] = entityManager.CreateEntity();

            T component = componentDeserializer.Deserialize<T>();

            entityManager.AddComponentData(entityMap[entityId], component);

            if (component is IEnableableComponent)
                entityManager.SetComponentEnabled(entityMap[entityId], typeof(T), save.enabled[i]);
        }
    }

    private TypeManager.TypeInfo GetTypeInfo(string name)
    {
        foreach (var typeInfo in types)
        {
            SaveAttribute saveAttribute = (SaveAttribute)typeInfo.Type.GetCustomAttribute(typeof(SaveAttribute));

            if (saveAttribute == null)
            {
                if (name == typeInfo.Type.Name)
                    return typeInfo;
            }
            else
            {
                if (name == saveAttribute.name)
                    return typeInfo;
            }
        }
        throw new($"ComponentType \"{name}\" does not exist");
    }
}