using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct Tile
{
    public List<(IComponentData, bool)> components;
    public List<(ComponentType, bool)> tags;
    public Tile(params IComponentData[] _components)
    {
        components = new(_components.Length);
        foreach (var component in _components)
            components.Add((component, true));
        tags = new();
    }
    public readonly T GetComponent<T>() where T : IComponentData
    {
        foreach (var component in components)
            if (component.GetType() == typeof(T))
                return (T)component.Item1;
        Debug.LogError($"Tile does not contain component of type {typeof(T).Name}");
        return default;
    }
}
public enum Components : int
{
    MapTileComponent = 0,
    AgingTile = 1,
    ElectricityProducer = 2,
    ElectricityConsumer = 3
}