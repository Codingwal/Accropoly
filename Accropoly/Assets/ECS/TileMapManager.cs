using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Rendering;

public class TileMapManager : MonoBehaviour
{
    private const float tileSize = 30;
    public Entity[,] map;
    EntityManager entityManager;

    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;

    private void Awake()
    {
        Debug.Assert(World.DefaultGameObjectInjectionWorld != null);
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }
    public void GenerateTileMap(Serializable2DArray<Tile> selectedMap)
    {
        Vector2 mapSize = new(selectedMap.GetLength(0), selectedMap.GetLength(1));

        map = new Entity[(int)mapSize.x, (int)mapSize.y];

        EntityArchetype entityArchetype = entityManager.CreateArchetype(
            typeof(MapTileComponent),
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld)
        );

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                float worldPosX = (x - mapSize.x * 0.5f + 0.5f) * tileSize;
                float worldPosZ = (y - mapSize.y * 0.5f + 0.5f) * tileSize;

                // Tile tile = selectedMap[i, j];


                map[x, y] = entityManager.CreateEntity(entityArchetype);
                entityManager.SetComponentData(map[x, y], new Translation { Value = new(worldPosX, 0, worldPosZ) });
                entityManager.SetComponentData(map[x, y], new MapTileComponent(x, y));
                entityManager.SetSharedComponentData(map[x, y], new RenderMesh { mesh = mesh, material = material });
            }
        }
    }
}
