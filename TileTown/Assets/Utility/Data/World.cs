using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct World
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

    public World(Serializable2DArray<Tile> map)
    {
        playTime = 0;

        cameraSystemPos = new();
        cameraSystemRotation = Quaternion.identity;
        followOffsetY = 50;

        balance = 500;

        population = new();

        this.map = map;
    }
}
