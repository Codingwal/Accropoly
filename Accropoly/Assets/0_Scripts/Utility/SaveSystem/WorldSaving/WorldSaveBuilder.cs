using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using ComponentSave = WorldSave.ComponentSave;

public class WorldSaveBuilder
{
    private Dictionary<Type, int> componentSaveIndices;
    private WorldSave save;
    private int entityCount;
    private List<TypeManager.TypeInfo> saveableTypes;
    public WorldSaveBuilder()
    {
        componentSaveIndices = new();
        save = new() { componentSaves = new(5, Allocator.Persistent) };
        entityCount = 0;
        saveableTypes = SaveComponents.GetSaveComponentTypeInfos();
    }

    public WorldSave GetWorldSave()
    {
        return save;
    }

    public int CreateEntity()
    {
        entityCount++;
        return entityCount - 1;
    }

    public unsafe void AddComponent<T>(int entityId, T data, bool enabled = true)
        where T : unmanaged, IComponentData
    {
        int index = GetOrCreateComponentSave(typeof(T));

        ref ComponentSave componentSave = ref save.componentSaves.ElementAt(index);
        componentSave.entityIds.Add(entityId);
        componentSave.components.AddRange(&data, sizeof(T));
        componentSave.enabled.Add(enabled);
    }

    private int GetOrCreateComponentSave(Type type)
    {
        var typeInfo = TypeManager.GetTypeInfo(TypeManager.GetTypeIndex(type));
        Debug.Assert(saveableTypes.Contains(typeInfo), $"Can't save non saveable component type {type}");

        if (componentSaveIndices.TryGetValue(type, out int index))
            return index;

        ComponentSave componentSave = new()
        {
            name = type.Name,
            entityIds = new(0, Allocator.Persistent),
            components = new(0, Allocator.Persistent),
            enabled = new(0, Allocator.Persistent)
        };

        save.componentSaves.Add(componentSave);
        componentSaveIndices.Add(type, save.componentSaves.Length - 1);
        return save.componentSaves.Length - 1;
    }
}