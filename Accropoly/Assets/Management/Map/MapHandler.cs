using System.Collections.Generic;
using UnityEngine;
using System;
using System.Security.Principal;
using UnityEngine.Tilemaps;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities.UniversalDelegates;

public struct TilePrefabContainer : IComponentData
{
    public Entity prefab;
}
public class MapHandler : MonoBehaviour
{
    public float tileSize;
    public Transform tileParent;
    public GameObject tilePrefab;

    public Serializable2DArray<Entity> map;

    public class Baker : Baker<MapHandler>
    {
        public override void Bake(MapHandler authoring)
        {
            Debug.Log("Bake");
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new TilePrefabContainer { prefab = GetEntity(authoring.tilePrefab, TransformUsageFlags.Dynamic) });
        }
    }

    // public static void GenerateTileMap(Serializable2DArray<Tile> selectedMap)
    // {
    //     int amountToSpawn = selectedMap.GetLength(0) * selectedMap.GetLength(1);
    //     EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

    //     EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(TilePrefabContainer));
    //     Entity prefab = entityQuery.GetSingleton<TilePrefabContainer>().prefab;
    //     // TODO: This throws an error when running the game, it may be required to retrieve the prefab differently

    //     NativeArray<Entity> spawnedEntities = new(amountToSpawn, Allocator.Temp);
    //     entityManager.Instantiate(prefab, spawnedEntities);

    //     for (int x = 0; x < selectedMap.GetLength(0); x++)
    //     {
    //         for (int y = 0; y < selectedMap.GetLength(1); y++)
    //         {
    //             int i = x * selectedMap.GetLength(0) + y;
    //             Entity entity = spawnedEntities[i];

    //             entityManager.SetComponentData(entity, LocalTransform.FromPosition(2 * x, 1, 2 * y));
    //         }
    //     }
    //     spawnedEntities.Dispose();
    // }

    public static Serializable2DArray<Tile> SaveTileMap()
    {
        return null;
        /*
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
        */
    }

}
/*
public Vector2 GetTilePosFromNeighbour(Vector2 position, float direction)
{
    direction = (int)Math.Round(direction);
    while (direction >= 360)
    {
        direction -= 360;
    }
    while (direction < 0)
    {
        direction += 360;
    }

    return (float)(direction / 90) switch
    {
        0 => new(position.x, position.y + 1),
        1 => new(position.x + 1, position.y),
        2 => new(position.x, position.y - 1),
        3 => new(position.x - 1, position.y),
        _ => throw new Exception("This direction does not exist: " + direction),
    };
}

*/