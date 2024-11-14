using Unity.Entities;
using Unity.Mathematics;
public struct MapTileComponent : IComponentData
{
    public TileType tileType;
    public int2 pos;
    public Direction rotation;
    public MapTileComponent(int x, int y, TileType tileType, Direction rotation)
    {
        pos = new(x, y);
        this.tileType = tileType;
        this.rotation = rotation;
    }
}
// Explicit values for compatability with other versions (new TileTypes just get a higher value)
public enum TileType
{
    // Natural
    Plains = 0,
    Sapling = 1,
    Forest = 2,
    Water = 6,

    // Streets
    Street = 3,

    // Habitats
    House = 4,
    Hut = 7,

    // Energy production
    SolarPanel = 5,
}