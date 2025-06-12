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
    // Natural
    Plains = 0,
    Sapling = 1,
    GrowingForest = 10,
    Forest = 2,
    Lake = 6,
    River = 7,

    // Streets
    Street = 3,

    // Habitats
    House = 4,
    Hut = 8,

    // Workplaces
    Office = 9,

    // Energy production
    SolarPanel = 5,
}