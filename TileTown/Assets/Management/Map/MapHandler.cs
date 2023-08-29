using System.Collections.Generic;
using UnityEngine;
using System;

public class MapHandler : Singleton<MapHandler>
{
    public float tileSize = 30;
    public Transform tileParent;

    [Header("Tile prefabs dictionary")]
    public List<TileType> tilePrefabsDictKeys;
    public List<GameObject> tilePrefabsDictValues;
    public SerializableDictionary<TileType, GameObject> tilePrefabs;

    public Serializable2DArray<GameObject> map;


    public void GenerateTileMap(Serializable2DArray<Tile> selectedMap)
    {
        if (tileParent == null)
        {
            tileParent = GameObject.Find("TileMap").transform;
        }

        // Delete all existing childs
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

        map = new Serializable2DArray<GameObject>((int)mapSize.x, (int)mapSize.y);

        for (int i = 0; i < mapSize.x; i++)
        {
            for (int j = 0; j < mapSize.y; j++)
            {
                float worldPosX = (i - mapSize.x * 0.5f + 0.5f) * tileSize;
                float worldPosZ = (j - mapSize.y * 0.5f + 0.5f) * tileSize;

                Tile tile = selectedMap[i, j];
                GameObject tilePrefab = tilePrefabsDictValues[tilePrefabsDictKeys.IndexOf(tile.tileType)];

                map[i, j] = GenerateTile(tilePrefab, tileSize, new(worldPosX, 0, worldPosZ), tile.direction * 90, i, j);
            }
        }
    }
    private GameObject GenerateTile(GameObject tilePrefab, float tileSize, Vector3 position, int rotation, int indexX, int indexY)
    {
        GameObject tile = Instantiate(tilePrefab, tileParent);
        tile.transform.position = position;
        tile.transform.eulerAngles = new(0, rotation, 0);
        tile.transform.localScale = new(tileSize, 1, tileSize);

        IMapTile mapTileScript = tile.GetComponent<IMapTile>();
        mapTileScript.X = indexX;
        mapTileScript.Y = indexY;

        return tile;
    }

    public Serializable2DArray<Tile> SaveTileMap()
    {
        int tileMapSize = (int)Math.Sqrt(tileParent.childCount);
        Serializable2DArray<Tile> tilemap = new(tileMapSize, tileMapSize);

        for (int i = 0; i < tileMapSize; i++)
        {
            for (int j = 0; j < tileMapSize; j++)
            {
                Transform child = tileParent.GetChild(i * tileMapSize + j);

                tilemap[i, j] = child.GetComponent<IMapTile>().GetTile();
            }
        }

        return tilemap;
    }
}
