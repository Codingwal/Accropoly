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

        foreach (var typeInfo in types)
        {
            if (typeInfo.Category == TypeManager.TypeCategory.ComponentData)
            {
                // this.SaveComponent<type.Type>();
                var componentSave = (WorldSave.ComponentSave?)GetType().GetMethod(nameof(SaveComponent), BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(typeInfo.Type)
                    .Invoke(this, new object[] { });

                if (componentSave.HasValue) // Skip components with zero entities
                    save.componentSaves.Add(componentSave.Value);
            }
            else if (typeInfo.Category == TypeManager.TypeCategory.BufferData)
            {
                // this.SaveBuffer<type.Type>();
                var componentSave = (WorldSave.ComponentSave?)GetType().GetMethod(nameof(SaveBuffer), BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(typeInfo.Type)
                    .Invoke(this, new object[] { });

                if (componentSave.HasValue) // Skip components with zero entities
                    save.componentSaves.Add(componentSave.Value);
            }
            else
                Debug.LogWarning("Skipping type that is neither a component nor a buffer");
        }

        return save;
    }

    private WorldSave.ComponentSave? SaveBuffer<T>()
        where T : unmanaged, IBufferElementData
    {
        EntityQuery query = new EntityQueryBuilder(Allocator.Temp).WithPresent<T>().Build(entityManager);

        // Skip components with zero entities
        if (query.CalculateEntityCount() == 0)
            return null;

        WorldSave.ComponentSave componentSave = new()
        {
            name = GetName(typeof(T)),
            entityIds = new(Allocator.Persistent),
            components = new(Allocator.Persistent),
            enabled = new(Allocator.Persistent)
        };

        Serializer componentSerializer = new(new NativeListWriter(componentSave.components));

        NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);

        foreach (Entity entity in entities)
        {
            componentSave.entityIds.Add(entity.Index);

            var buffer = entityManager.GetBuffer<T>(entity);

            componentSerializer.Serialize(buffer.Length);
            foreach (T element in buffer)
                componentSerializer.Serialize(element);

            componentSave.enabled.Add(true);
        }

        return componentSave;
    }
    private WorldSave.ComponentSave? SaveComponent<T>()
        where T : unmanaged, IComponentData
    {
        EntityQuery query = new EntityQueryBuilder(Allocator.Temp).WithPresent<T>().Build(entityManager);

        // Skip components with zero entities
        if (query.CalculateEntityCount() == 0)
            return null;

        WorldSave.ComponentSave componentSave = new()
        {
            name = GetName(typeof(T)),
            entityIds = new(Allocator.Persistent),
            components = new(Allocator.Persistent),
            enabled = new(Allocator.Persistent)
        };

        Serializer componentSerializer = new(new NativeListWriter(componentSave.components));

        NativeArray<T> components = query.ToComponentDataArray<T>(Allocator.Temp);
        NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);

        bool isEnableable = TypeManager.GetTypeIndex(typeof(T)).IsEnableable;

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