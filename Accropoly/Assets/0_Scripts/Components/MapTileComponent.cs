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
    public static Entity GetTile(int2 pos)
    {
        var buffer = GetEntityGrid();
        return buffer[pos.x * (int)math.sqrt(buffer.Length) + pos.y];
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