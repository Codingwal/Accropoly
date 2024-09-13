using Unity.Entities;
using Unity.Mathematics;
public struct MapTileComponent : IComponentData
{
    public TileType tileType;
    public int2 pos;
    public MapTileComponent(int x, int y, TileType tileType)
    {
        pos = new(x, y);
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