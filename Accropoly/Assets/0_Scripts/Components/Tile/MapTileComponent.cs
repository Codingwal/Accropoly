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
// Explicit values for compatability with other versions (new TileTypes just get a higher value)
public enum TileType
{
    // Naturally generated
    Plains = 0,
    Sapling = 1,
    Forest = 2,

    // Streets
    Street = 3,
    StreetCorner = 4,
    StreetTJunction = 5,
    StreetJunction = 6,

    // Houses
    House = 7,
    Skyscraper = 8,

    // Energy production
    SolarPanel = 9,
    CoalPowerPlant = 10,

    // Workplace
    OfficeBuilding = 11
}