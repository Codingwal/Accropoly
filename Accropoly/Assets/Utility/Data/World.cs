using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct WorldData
{
    // Time
    public float playTime;

    // Camera system
    public float2 cameraSystemPos;
    public quaternion cameraSystemRotation;
    public float followOffsetY;

    // Economy system
    public float balance;

    // Population system
    public List<PersonData> population;

    // Tilemap system
    public MapData map;

    public WorldData(MapData map)
    {
        playTime = 0;

        cameraSystemPos = new();
        cameraSystemRotation = Quaternion.identity;
        followOffsetY = 100;

        balance = 10000;

        population = new();

        this.map = map;
    }
}
