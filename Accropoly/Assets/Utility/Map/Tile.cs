using System.Collections.Generic;
using Unity.Entities;

[System.Serializable]
public struct Tile
{
    public Dictionary<ComponentType, IComponentData> components;
}