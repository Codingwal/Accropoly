using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameLoopManager : Singleton<GameLoopManager>
{
    [SerializeField] private float mapSize = 50;
    [SerializeField] private float tileSize = 30;
    [SerializeField] private Vector2 center = new(0, 0);

    [SerializeField] private Dictionary<TileType, GameObject> tilePrefabs;

    [Header("Temperorary")]
    [SerializeField] private bool generateTileMap;

    protected override void SingletonAwake()
    {
        if (generateTileMap)
        {
            CreateTileMap(mapSize, tileSize, center);
        }
    }
    private void CreateTileMap(float mapSize, float tileSize, Vector2 center)
    {
        for (int i = 0; i < mapSize; i++)
        {
            for (int j = 0; j < mapSize; j++)
            {
                float worldPosX = (i - mapSize * 0.5f + center.x + 0.5f) * tileSize;
                float worldPosZ = (j - mapSize * 0.5f + center.y + 0.5f) * tileSize;

                GenerateTile(tileSize, new(worldPosX, 0, worldPosZ));
            }
        }
    }
    private GameObject SelectTileType()
    {
        int randomNumber = Random.Range(1, 100);

        // Add tile selection logic and return the tile type


    }
    private void GenerateTile(float tileSize, Vector3 position)
    {
        GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
    }
}

public enum TileType
{
    // Naturally generated
    Grass,
    Forest,

    // // Normal street, houses can connect
    // Street,
    // StreetCorner,

    // // Faster street, houses can't connect
    // Highway,
    // HighwayCorner,
}
