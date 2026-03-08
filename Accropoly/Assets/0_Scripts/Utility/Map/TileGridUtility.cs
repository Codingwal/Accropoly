using Unity.Entities;
using Unity.Mathematics;
using Components;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

public static class TileGridUtility
{
    public static DynamicBuffer<EntityBufferElement> CreateEntityGridBuffer()
    {
        var em = ECSUtility.EntityManager;
        Entity entity = em.CreateEntity();
        em.AddComponent<Tags.EntityGridHolder>(entity);
        return em.AddBuffer<EntityBufferElement>(entity);
    }
    /// <remarks>Can't be used in jobs! Read-only!</remarks>
    public static DynamicBuffer<EntityBufferElement> GetEntityGrid()
    {
        var em = ECSUtility.EntityManager;
        return em.GetBuffer<EntityBufferElement>(ECSUtility.GetSingletonEntity<Tags.EntityGridHolder>(), isReadOnly: true);
    }
    /// <remarks>Can't be used in jobs!</remarks>
    public static DynamicBuffer<EntityBufferElement> GetEntityGridRW()
    {
        var em = ECSUtility.EntityManager;
        return em.GetBuffer<EntityBufferElement>(ECSUtility.GetSingletonEntity<Tags.EntityGridHolder>());
    }
    public static int GetIndex(int2 pos, int totalMapSize)
    {
        if (TryGetIndex(pos, totalMapSize, out int index))
            return index;
        else
            throw new($"Invalid position {pos}");
    }
    public static bool TryGetIndex(int2 pos, int totalMapSize, out int index)
    {
        int length = (int)math.sqrt(totalMapSize);
        if (pos.x >= length || pos.x < 0 || pos.y >= length || pos.y < 0)
        {
            index = -1;
            return false;
        }
        index = pos.x * length + pos.y;
        return true;
    }
    /// <remarks>Can't be used in jobs!</remarks>
    public static Entity GetTile(int2 pos)
    {
        if (TryGetTile(pos, out Entity entity))
            return entity;
        else
            throw new($"Invalid position {pos}");
    }
    public static Entity GetTile(int2 pos, DynamicBuffer<EntityBufferElement> buffer)
    {
        if (TryGetTile(pos, buffer, out Entity entity))
            return entity;
        else
            throw new($"Invalid position {pos}");
    }
    /// <remarks>Can't be used in jobs!</remarks>
    public static bool TryGetTile(int2 pos, out Entity entity) { return TryGetTile(pos, GetEntityGrid(), out entity); }
    public static bool TryGetTile(int2 pos, DynamicBuffer<EntityBufferElement> buffer, out Entity entity)
    {
        if (TryGetIndex(pos, buffer.Length, out int index))
        {
            entity = buffer[index];
            return true;
        }
        entity = default;
        return false;
    }
    public static NativeArray<Entity> GetSquareEdgeTiles(int2 pos, DynamicBuffer<EntityBufferElement> entityGrid)
    {
        return new NativeArray<Entity>(4, Allocator.Temp)
        {
            [0] = GetTile(pos + new int2(-1, 1), entityGrid),
            [1] = GetTile(pos + new int2(1, 1), entityGrid),
            [2] = GetTile(pos + new int2(1, -1), entityGrid),
            [3] = GetTile(pos + new int2(-1, -1), entityGrid)
        };
    }
}
