using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WorldData
{
    // Time
    public float playTime;

    // Camera system
    public Vector2 cameraSystemPos;
    public Quaternion cameraSystemRotation;
    public float followOffsetY;

    // Economy system
    public float balance;

    // Population system
    public List<PersonData> population;

    // Tilemap system
    public Serializable2DArray<Tile> map;

    public WorldData(Serializable2DArray<Tile> map)
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
