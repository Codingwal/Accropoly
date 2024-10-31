using System.Collections.Generic;
using Unity.Entities;

public static class TileTypeToComponents
{
    public static List<ComponentType> GetComponents(TileType tileType)
    {
        List<ComponentType> componentTypes = tileType switch
        {
            TileType.Plains => new() { },
            TileType.Sapling => new() { typeof(AgingTile) },
            TileType.Forest => new() { },
            _ => throw new($"Missing componentTypes for tileType {tileType}"),
        };
        componentTypes.Add(typeof(MapTileComponent));
        componentTypes.Add(typeof(ActiveTileTag));
        return componentTypes;
    }
}