using System.Collections.Generic;
using Unity.Mathematics;

[System.Serializable]
public struct WorldData
{
    // Time
    public float playTime;

    // Camera system
    public float2 cameraSystemPos;
    public quaternion cameraSystemRotation;
    public float cameraDistance;

    // Economy system
    public float balance;

    // Tilemap system
    public MapData map;

    public WorldData(MapData map)
    {
        playTime = 0;

        cameraSystemPos = new();
        cameraSystemRotation = quaternion.identity;
        cameraDistance = 30;

        balance = 10000;

        this.map = map;
    }
}

public struct MapData
{
    public Tile[,] tiles;
    public static explicit operator MapData(Serializable2DArray<Tile> array)
    {
        MapData data = new()
        {
            tiles = new Tile[array.GetLength(0), array.GetLength(1)]
        };
        for (int x = 0; x < array.GetLength(0); x++)
        {
            for (int y = 0; y < array.GetLength(1); y++)
            {
                data.tiles[x, y] = array[x, y];
            }
        }
        return data;
    }
}