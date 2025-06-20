using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    public struct Tile : IComponentData
    {
        public TileType tileType;
        public int2 pos;
        public Direction rotation;
        public Tile(int x, int y, TileType tileType, Direction rotation)
        {
            pos = new(x, y);
            this.tileType = tileType;
            this.rotation = rotation;
        }
    }
}
// Explicit values for compatability with other versions (new TileTypes just get a higher value)
public enum TileType
{
    None = 0,

    // Natural
    Plains = 1,
    Sapling = 2,
    Forest = 3,
    Lake = 7,
    River = 8,
    GrowingForest = 12,

    // Streets
    Street = 4,

    // Habitats
    House = 5,
    Hut = 9,

    // Workplaces
    Office = 10,

    // Energy production
    SolarPanel = 6,
    WindTurbine = 11,

    // City
    Bitumen = 20,
    CityStreet = 21
}