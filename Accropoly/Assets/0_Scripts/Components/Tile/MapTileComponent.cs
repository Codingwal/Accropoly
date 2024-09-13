using Unity.Entities;
using Unity.Mathematics;
public struct MapTileComponent : IComponentData
{
    public TileType tileType;
    public int2 pos;
    public int rotation;
    public MapTileComponent(int x, int y, TileType tileType, int rotation)
    {
        pos = new(x, y);
        this.tileType = tileType;
        this.rotation = rotation;
    }
    public void Rotate(int degrees)
    {
        rotation += degrees;
        if (rotation > 180)
            rotation -= 360;
    }
}
public enum TileType
{
    // Naturally generated
    Plains,
    Forest,

    // Streets
    Street,
    StreetCorner,
    StreetTJunction,
    StreetJunction,

    // Houses
    House,
    Skyscraper,

    // Energy production
    SolarPanel,
    CoalPowerPlant,

    // Workplace
    OfficeBuilding
}