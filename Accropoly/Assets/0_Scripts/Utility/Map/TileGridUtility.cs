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
    /// <remarks>Can't be used in jobs!</remarks>
    public static Entity GetTile(int2 pos)
    {
        Entity entity = TryGetTile(pos, out bool entityExists);
        if (!entityExists) throw new($"Invalid position {pos}");
        return entity;
    }
    /// <remarks>Can't be used in jobs!</remarks>
    public static Entity TryGetTile(int2 pos, out bool entityExists)
    {
        var buffer = GetEntityGrid();
        int index = GetIndex(pos, buffer.Length);
        if (index >= buffer.Length || index < 0)
        {
            entityExists = false;
            return new();
        }
        entityExists = true;
        return buffer[index];
    }
    public static Entity GetTile(int2 pos, DynamicBuffer<EntityBufferElement> buffer)
    {
        Entity entity = TryGetTile(pos, buffer, out bool entityExists);
        if (!entityExists) throw new($"Invalid position {pos}");
        return entity;
    }
    public static Entity TryGetTile(int2 pos, DynamicBuffer<EntityBufferElement> buffer, out bool entityExists)
    {
        int index = GetIndex(pos, buffer.Length);
        if (index >= buffer.Length || index < 0)
        {
            entityExists = false;
            return new();
        }
        entityExists = true;
        return buffer[index];
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
