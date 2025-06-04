using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct TileData
{
    public List<(IComponentData, bool)> components;
    public TileData(params IComponentData[] _components)
    {
        components = new(_components.Length);
        foreach (var component in _components)
            components.Add((component, true));
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
// Explicit values for compatability with other versions (new TileComponents just get a higher value)
public enum TileComponents : int
{
    // Components
    Tile = 0,
    AgingTile = 1,
    ElectricityProducer = 2,
    ElectricityConsumer = 3,
    ConnectingTile = 4,
    Polluter = 5,
    Habitat = 6,
    Employer = 7,
    TransportTile = 8,

    // Tags
    ActiveTileTag = 100,
    NewTileTag = 101,
    IsConnectedTag = 102,
    BuildingConnectorTag = 103,
    DisabledTileTag = 104,
}