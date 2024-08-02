using Unity.Entities;
using UnityEngine;

[assembly: RegisterGenericComponentType(typeof(MapTileComponent))]  
public struct MapTileComponent : IComponentData
{
    public Vector2Int pos;
    public MapTileComponent(int x, int y)
    {
        pos = new Vector2Int(x, y);
    }
}
