using Unity.Entities;
using Unity.Mathematics;

public static class TileGridUtility
{
    public static DynamicBuffer<EntityBufferElement> CreateEntityGridBuffer()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity entity = em.CreateEntity();
        em.AddComponent<EntityGridHolder>(entity);
        return em.AddBuffer<EntityBufferElement>(entity);
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
