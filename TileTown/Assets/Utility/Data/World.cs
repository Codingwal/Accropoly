using UnityEngine;

[System.Serializable]
public struct World
{
    // Camera transform
    public Vector2 cameraSystemPos;
    public Quaternion cameraSystemRotation;
    public float followOffsetY;

    public Serializable2DArray<Tile> map;

    public World(Serializable2DArray<Tile> map)
    {
        this.map = map;

        cameraSystemPos = new();
        cameraSystemRotation = Quaternion.identity;
        followOffsetY = 50;
    }
}
