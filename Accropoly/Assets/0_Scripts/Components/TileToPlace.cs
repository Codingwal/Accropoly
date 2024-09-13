using Unity.Entities;

public struct TileToPlace : IComponentData
{
    public TileType tileType;
    public int rotation;
    public void Rotate(int degrees)
    {
        rotation += degrees;
        if (rotation > 180)
            rotation -= 360;
    }
}