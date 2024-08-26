using Unity.Entities;
using Unity.Mathematics;

public struct MapTileComponent : IComponentData
{
    public TileType tileType;
    public float2 pos;
    public MapTileComponent(int x, int y, TileType tileType)
    {
        pos = new float2(x, y);
        this.tileType = tileType;
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