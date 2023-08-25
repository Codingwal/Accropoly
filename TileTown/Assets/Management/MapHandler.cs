using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapHandler : SingletonPersistant<MapHandler>
{
    [SerializeField] private float tileSize = 30;
    [SerializeField] private Transform tileParent;

    [Header("Tile prefabs dictionary")]
    [SerializeField] private List<TileType> tilePrefabsDictKeys;
    [SerializeField] private List<GameObject> tilePrefabsDictValues;

    private static readonly TileType p = TileType.Plains;
    private static readonly TileType f = TileType.Forest;


    public static Dictionary<string, TileType[][]> mapTemplates = new() {
        {
            "DefaultMap",
            new TileType[][]
            {
                new TileType[] { p, p, p, f, p},
                new TileType[] { p, f, f, p, p},
                new TileType[] { p, p, f, p, f},
                new TileType[] { p, p, f, p, p},
                new TileType[] { p, p, p, p, p},
            }
        },
        {
            "PlainsMap",
            new TileType[][]
            {
                new TileType[] { p, p, p, p, p},
                new TileType[] { p, p, p, p, p},
                new TileType[] { p, p, p, p, p},
                new TileType[] { p, p, p, p, p},
                new TileType[] { p, p, p, p, p},
            }
        },
        {
            "ForestMap",
            new TileType[][]
            {
                new TileType[] { f, f, f, f, f},
                new TileType[] { f, f, f, f, f},
                new TileType[] { f, f, f, f, f},
                new TileType[] { f, f, f, f, f},
                new TileType[] { f, f, f, f, f},
            }
        },
    };

    public void GenerateTileMap(Serializable2DArray<TileType> selectedMap)
    {
        if (tileParent == null)
        {
            tileParent = GameObject.Find("TileMap").transform;
        }
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
    public Serializable2DArray<TileType> SaveTileMap()
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

        return tilemap;
    }
}
