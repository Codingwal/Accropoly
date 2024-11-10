using Unity.Entities;

public struct TileToPlace : IComponentData
{
    public TileType tileType;
    public Direction rotation;
}