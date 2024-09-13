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
    public static DynamicBuffer<EntityBufferElement> GetEntityGrid()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = em.CreateEntityQuery(typeof(EntityGridHolder));
        return em.GetBuffer<EntityBufferElement>(query.GetSingletonEntity());
    }
    public static int GetIndex(int2 pos, int totalMapSize)
    {
        return pos.x * (int)math.sqrt(totalMapSize) + pos.y;
    }
    public static Entity GetTile(int2 pos)
    {
        var buffer = GetEntityGrid();
        return buffer[GetIndex(pos, buffer.Length)];
    }
    public static Entity[] GetNeighbourTiles(int2 pos)
    {
        var buffer = GetEntityGrid();
        return new Entity[4]
        {
            buffer[GetIndex(pos + new int2(1, 0), buffer.Length)],
            buffer[GetIndex(pos + new int2(-1, 0), buffer.Length)],
            buffer[GetIndex(pos + new int2(0, 1), buffer.Length)],
            buffer[GetIndex(pos + new int2(0, -1), buffer.Length)],
        };
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