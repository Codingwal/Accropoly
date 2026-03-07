using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class WorldSaver
{
    public List<TypeManager.TypeInfo> types;
    private EntityManager entityManager;
    public WorldSaver(EntityManager _entityManager)
    {
        entityManager = _entityManager;

        types = SaveComponents.GetSaveComponentTypeInfos();
    }
    public WorldSave Save()
    {
        WorldSave save = new() { componentSaves = new(5, Allocator.Persistent) };

        foreach (var type in types)
        {
            // TODO: Skip components with zero entities

            // this.SaveComponent<type.Type>(entityManager);
            var componentSave = GetType().GetMethod(nameof(SaveComponent), BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(type.Type)
                .Invoke(this, new object[] { });

            save.componentSaves.Add((WorldSave.ComponentSave)componentSave);
        }

        return save;
    }

    private WorldSave.ComponentSave SaveComponent<T>()
        where T : unmanaged, IComponentData
    {
        EntityQuery query = new EntityQueryBuilder(Allocator.Temp).WithPresent<T>().Build(entityManager);
        WorldSave.ComponentSave componentSave = new()
        {
            entityIds = new(Allocator.Persistent),
            components = new(Allocator.Persistent),
            enabled = new(Allocator.Persistent)
        };

        Serializer componentSerializer = new(new NativeListWriter(componentSave.components));

        NativeArray<T> components = query.ToComponentDataArray<T>(Allocator.Temp);
        NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);

        bool isEnableable = TypeManager.GetTypeIndex(typeof(T)).IsEnableable;

        componentSave.name = GetName(typeof(T));

        for (int i = 0; i < components.Length; i++)
        {
            componentSave.entityIds.Add(entities[i].Index);

            componentSerializer.Serialize(components[i]);

            // If isEnableable, check if enabled (otherwise just set as true)
            componentSave.enabled.Add(!isEnableable || entityManager.IsComponentEnabled(entities[i], typeof(T)));
        }

        return componentSave;
    }

    private string GetName(Type type)
    {
        SaveAttribute saveAttribute = (SaveAttribute)type.GetCustomAttribute(typeof(SaveAttribute));

        if (saveAttribute == null)
            return type.Name;

        return saveAttribute.name;
    }
}