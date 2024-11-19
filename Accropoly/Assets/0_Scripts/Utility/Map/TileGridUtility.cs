using Unity.Entities;
using Unity.Mathematics;
using Components;

public static class TileGridUtility
{
    public static DynamicBuffer<EntityBufferElement> CreateEntityGridBuffer()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity entity = em.CreateEntity();
        em.AddComponent<Tags.EntityGridHolder>(entity);
        return em.AddBuffer<EntityBufferElement>(entity);
    }
    public static DynamicBuffer<EntityBufferElement> GetEntityGrid()
    {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = em.CreateEntityQuery(typeof(Tags.EntityGridHolder));
        return em.GetBuffer<EntityBufferElement>(query.GetSingletonEntity());
    }
    public static int GetIndex(int2 pos, int totalMapSize)
    {
        return pos.x * (int)math.sqrt(totalMapSize) + pos.y;
    }
    /// <remarks>Can't be used in jobs!</remarks>
    public static Entity GetTile(int2 pos)
    {
        if (TryGetTile(pos, out Entity entity))
            return entity;
        else
            throw new($"Invalid position {pos}");
    }
    /// <remarks>Can't be used in jobs!</remarks>
    public static Entity GetTile(int2 pos, DynamicBuffer<EntityBufferElement> buffer)
    {
        if (TryGetTile(pos, buffer, out Entity entity))
            return entity;
        else
            throw new($"Invalid position {pos}");
    }
    public static bool TryGetTile(int2 pos, out Entity entity) { return TryGetTile(pos, GetEntityGrid(), out entity); }
    public static bool TryGetTile(int2 pos, DynamicBuffer<EntityBufferElement> buffer, out Entity entity)
    {
        int index = GetIndex(pos, buffer.Length);
        if (index >= buffer.Length || index < 0)
        {
            entity = default;
            return false;
        }
        entity = buffer[index];
        return true;
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
