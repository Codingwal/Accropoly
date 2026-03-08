using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using ComponentSave = WorldSave.ComponentSave;

public class WorldSaveBuilder
{
    private Dictionary<Type, int> componentSaveIndices;
    private WorldSave save;
    private List<Serializer> serializers;
    private int entityCount;
    private List<TypeManager.TypeInfo> saveableTypes;
    public WorldSaveBuilder()
    {
        componentSaveIndices = new();
        save = new() { componentSaves = new(5, Allocator.Persistent) };
        serializers = new();
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

    public void AddComponent<T>(int entityId, T data, bool enabled = true)
        where T : unmanaged, IComponentData
    {
        int index = GetOrCreateComponentSave(typeof(T));
        ref ComponentSave componentSave = ref save.componentSaves.ElementAt(index);

        componentSave.entityIds.Add(entityId);
        serializers[index].Serialize(data);
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
            name = GetName(type),
            entityIds = new(0, Allocator.Persistent),
            components = new(0, Allocator.Persistent),
            enabled = new(0, Allocator.Persistent)
        };

        Serializer componentSerializer = new(new NativeListWriter(componentSave.components));

        save.componentSaves.Add(componentSave);
        serializers.Add(componentSerializer);
        componentSaveIndices.Add(type, save.componentSaves.Length - 1);

        return save.componentSaves.Length - 1;
    }

    private string GetName(Type type)
    {
        SaveAttribute saveAttribute = (SaveAttribute)type.GetCustomAttribute(typeof(SaveAttribute));

        if (saveAttribute == null)
            return type.Name;

        return saveAttribute.name;
    }
}