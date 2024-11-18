using System.Collections.Generic;
using Unity.Mathematics;

[System.Serializable]
public struct WorldData
{
    // Time
    public WorldTime time;

    // Camera system
    public float2 cameraSystemPos;
    public quaternion cameraSystemRotation;
    public float cameraDistance;

    // Economy system
    public float balance;

    // Population
    public List<PersonData> population;

    // Tilemap system
    public MapData map;

    public WorldData(MapData map)
    {
        time = new();

        cameraSystemPos = new();
        cameraSystemRotation = quaternion.identity;
        cameraDistance = 30;

        balance = 5000;

        population = new();

        this.map = map;
    }
}

public struct MapData
{
    public TileData[,] tiles;
}