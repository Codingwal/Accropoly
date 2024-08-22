using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor.Build.Pipeline;
using UnityEngine;

public struct MapData : IComponentData
{
    public int2 size;
    public NativeArray<Tile> tiles;
    public readonly int TotalSize => size.x * size.y;
    public readonly int GetIndex(int x, int y)
    {
        return x * size.y + y;
    }
    public static explicit operator MapData(Serializable2DArray<Tile> array)
    {
        MapData data = new()
        {
            size = new(array.GetLength(0), array.GetLength(1))
        };
        data.tiles = new(data.TotalSize, Allocator.Persistent);
        for (int x = 0; x < data.size.x; x++)
        {
            for (int y = 0; y < data.size.y; y++)
            {
                data.tiles[data.GetIndex(x, y)] = array[x, y];
            }
        }
        return data;
    }
}