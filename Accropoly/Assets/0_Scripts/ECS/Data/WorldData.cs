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

    // Population
    public List<Person> population;

    // Tilemap system
    public MapData map;

    public WorldData(MapData map)
    {
        playTime = 0;

        cameraSystemPos = new();
        cameraSystemRotation = quaternion.identity;
        cameraDistance = 30;

        balance = 10000;

        population = new();

        this.map = map;
    }
}

public struct MapData
{
    public Tile[,] tiles;
}