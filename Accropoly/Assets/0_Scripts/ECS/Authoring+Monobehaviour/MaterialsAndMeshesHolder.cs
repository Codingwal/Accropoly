using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public class MaterialsAndMeshesHolder : MonoBehaviour
{
    private static MaterialsAndMeshesHolder instance = null;
    [SerializeField] private List<SerializableKeyValuePair<TileType, MaterialMeshPair>> materialsAndMeshes = new();

    private static List<KeyValuePair<Entity, MaterialMeshPair>> entitiesToUpdate = new();
    private void Awake()
    {
        if (instance != null) Debug.LogError("Found multiple instances of MaterialsAndMeshesHolder");
        instance = this;
    }
    private void Update()
    {
        if (entitiesToUpdate.Count == 0) return;
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        foreach (var (entity, materialMeshPair) in entitiesToUpdate)
        {
            if (!entityManager.Exists(entity)) continue;

            entityManager.SetSharedComponentManaged(entity, new RenderMeshArray(
                new Material[] { materialMeshPair.material },
                new Mesh[] { materialMeshPair.mesh }
            ));
        }
        entitiesToUpdate.Clear();
    }
    public static MaterialMeshPair GetMaterialAndMesh(TileType tileType)
    {
        if (instance == null) Debug.LogError("Instance == null");

        foreach (var pair in instance.materialsAndMeshes)
            if (pair.key == tileType)
            {
                Debug.Assert(pair.value.material != null, $"Material for tileType {tileType} is null");
                Debug.Assert(pair.value.mesh != null, $"Mesh for tileType {tileType} is null");
                return pair.value;
            }
        Debug.LogError($"Material & Mesh for tileType {tileType} is missing");
        return default;
    }
    public static void UpdateMeshAndMaterial(Entity entity, MaterialMeshPair materialMeshPair)
    {
        entitiesToUpdate.Add(new(entity, materialMeshPair));
    }
    public static void UpdateMeshAndMaterial(Entity entity, TileType newTileType)
    {
        entitiesToUpdate.Add(new(entity, GetMaterialAndMesh(newTileType)));
    }
}
