using System.Collections.Generic;
using UnityEngine;

public class MaterialsAndMeshesHolder : MonoBehaviour
{
    private static MaterialsAndMeshesHolder instance = null;
    [SerializeField] private List<SerializableKeyValuePair<TileType, MaterialMeshPair>> materialsAndMeshes = new();
    private void Awake()
    {
        if (instance != null) Debug.LogError("Found multiple instances of TileSpawnerConfigAuthoring");
        instance = this;
        Debug.Log("Awake");
    }
    public static MaterialMeshPair GetMaterialAndMesh(TileType tileType)
    {
        if (instance == null) Debug.LogError("Instance == null");
        foreach (var pair in instance.materialsAndMeshes)
            if (pair.key == tileType)
                return pair.value;
        Debug.LogError($"Material & Mesh for tileType {tileType} is missing");
        return default;
    }
}
