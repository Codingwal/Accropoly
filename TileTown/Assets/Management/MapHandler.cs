using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapHandler : Singleton<MapHandler>
{
    [SerializeField] private float tileSize = 30;

    [SerializeField] private Transform tileParent;

    [Header("Tile prefabs dictionary")]
    [SerializeField] private List<TileType> tilePrefabsDictKeys;
    [SerializeField] private List<GameObject> tilePrefabsDictValues;

    [Header("Saved maps dictionary")]
    [SerializeField] private List<string> savedMapsDictKeys;
    [SerializeField] private List<Serializable2DArray<TileType>> savedMapsDictValues;

    private Serializable2DArray<TileType> selectedMap = null;

    [Header("For saving / generating map")]
    public string mapName;

    public void GenerateTileMap()
    {
        selectedMap = savedMapsDictValues[savedMapsDictKeys.IndexOf(mapName)];

        List<GameObject> childs = new();
        foreach (Transform child in tileParent)
        {
            childs.Add(child.gameObject);
        }
        foreach (GameObject child in childs)
        {
            DestroyImmediate(child);
        }

        Vector2 mapSize = new(selectedMap.GetLength(0), selectedMap.GetLength(1));

        for (int i = 0; i < mapSize.x; i++)
        {
            for (int j = 0; j < mapSize.y; j++)
            {
                float worldPosX = (i - mapSize.x * 0.5f + 0.5f) * tileSize;
                float worldPosZ = (j - mapSize.y * 0.5f + 0.5f) * tileSize;

                TileType tileType = selectedMap[i, j];
                GameObject tile = tilePrefabsDictValues[tilePrefabsDictKeys.IndexOf(tileType)];

                GenerateTile(tile, tileSize, new(worldPosX, 0, worldPosZ));
            }
        }
    }
    private void GenerateTile(GameObject tilePrefab, float tileSize, Vector3 position)
    {
        GameObject tile = Instantiate(tilePrefab, tileParent);
        tile.transform.position = position;
        tile.transform.localScale = new(tileSize, 1, tileSize);
    }
    public void SaveTileMap()
    {
        int tileMapSize = (int)Math.Sqrt(tileParent.childCount);
        Serializable2DArray<TileType> tilemap = new(tileMapSize, tileMapSize);

        for (int i = 0; i < tileMapSize; i++)
        {
            for (int j = 0; j < tileMapSize; j++)
            {
                Transform child = tileParent.GetChild(i * tileMapSize + j);

                tilemap[i, j] = child.GetComponent<IMapTile>().GetTileType();
            }
        }
        savedMapsDictKeys.Add(mapName);
        savedMapsDictValues.Add(tilemap);
    }
}