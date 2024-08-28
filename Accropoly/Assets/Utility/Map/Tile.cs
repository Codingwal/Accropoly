using System.Collections.Generic;
using System.Linq;
using Unity.Entities;

[System.Serializable]
public struct Tile
{
    public List<IComponentData> components;
    public Tile(params IComponentData[] components)
    {
        this.components = components.ToList();
    }
}
public enum Components
{
    MapTileComponent,
}