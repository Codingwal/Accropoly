using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

public struct Tile
{
    public List<IComponentData> components;
    public Tile(params IComponentData[] components)
    {
        this.components = components.ToList();
    }
    public readonly T GetComponent<T>() where T : IComponentData
    {
        foreach (var component in components)
            if (component.GetType() == typeof(T))
                return (T)component;
        Debug.LogError($"Tile does not contain component of type {typeof(T).Name}");
        return default;
    }
}
public enum Components : int
{
    MapTileComponent = 0,
    AgingTile = -1,
}