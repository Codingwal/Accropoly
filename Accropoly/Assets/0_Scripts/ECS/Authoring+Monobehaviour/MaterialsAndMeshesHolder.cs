using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class MaterialsAndMeshesHolder : MonoBehaviour
{
    private static MaterialsAndMeshesHolder instance = null;
    [SerializeField] private SerializableDictionary<TileType, MaterialMeshPair> simpleTiles = new();
    [SerializeField] private SerializableDictionary<TileType, MaterialMeshPairSet> connectingTiles = new();

    private NativeHashMap<int, MaterialMeshInfo> simpleTilesUnmanaged; // <TileType, pair>
    private NativeHashMap<int, MaterialMeshInfoSet> connectingTilesUnmanaged; // <TileType, set>

    private void Awake()
    {
        if (instance != null) Debug.LogError("Found multiple instances of MaterialsAndMeshesHolder");
        instance = this;
    }
    private void Start()
    {
        var graphicsSystem = ECSUtility.World.GetOrCreateSystemManaged<EntitiesGraphicsSystem>();

        simpleTilesUnmanaged = new(4, Allocator.Persistent);
        foreach ((TileType tileType, MaterialMeshPair pair) in simpleTiles)
        {
            Debug.Assert(pair.material != null, $"Material for tileType {tileType} is null");
            Debug.Assert(pair.mesh != null, $"Mesh for tileType {tileType} is null");

            BatchMaterialID matID = graphicsSystem.RegisterMaterial(pair.material);
            BatchMeshID meshID = graphicsSystem.RegisterMesh(pair.mesh);

            simpleTilesUnmanaged.Add((int)tileType, new(matID, meshID));
        }

        connectingTilesUnmanaged = new(1, Allocator.Persistent);
        foreach ((TileType tileType, MaterialMeshPairSet managedSet) in connectingTiles)
        {
            MaterialMeshInfoSet set = new(6, Allocator.Persistent);
            for (int i = 0; i < 6; i++)
            {
                MaterialMeshPair pair = managedSet.pairs[i];

                Debug.Assert(pair.material != null, $"Material for tileType {tileType} is null");
                Debug.Assert(pair.mesh != null, $"Mesh for tileType {tileType} is null");

                BatchMaterialID matID = graphicsSystem.RegisterMaterial(pair.material);
                BatchMeshID meshID = graphicsSystem.RegisterMesh(pair.mesh);

                set.pairs[i] = new(matID, meshID);
            }
            connectingTilesUnmanaged.Add((int)tileType, set);
        }
    }
    private void OnDestroy()
    {
        foreach (var pair in connectingTilesUnmanaged)
        {
            pair.Value.pairs.Dispose();
        }

        simpleTilesUnmanaged.Dispose();
        connectingTilesUnmanaged.Dispose();
    }
    public static MaterialMeshInfo GetMaterialAndMesh(TileType tileType)
    {
        if (instance == null) Debug.LogError("Instance == null");
        if (instance.simpleTilesUnmanaged.TryGetValue((int)tileType, out var pair))
            return pair;
        else if (instance.connectingTilesUnmanaged.TryGetValue((int)tileType, out var set))
            return set.pairs[0];
        else Debug.LogError($"Material & Mesh for tileType {tileType} is missing");
        return default;
    }
    public static void UpdateMeshAndMaterial(Entity entity, MaterialMeshInfo materialMeshInfo)
    {
        ECSUtility.EntityManager.SetComponentData(entity, materialMeshInfo);
    }
    public static void UpdateMeshAndMaterial(Entity entity, TileType newTileType)
    {
        UpdateMeshAndMaterial(entity, GetMaterialAndMesh(newTileType));
    }
    public static void UpdateAppearence(Entity entity, TileType tileType, Components.ConnectingTile connectingTile)
    {
        if (instance.connectingTilesUnmanaged.TryGetValue((int)tileType, out var set))
        {
            int index = connectingTile.GetIndex();
            var pair = set.pairs[index];

            UpdateMeshAndMaterial(entity, pair);
        }
        else Debug.LogError($"Material & Mesh for tileType {tileType} is missing (Assumed to be a connectingTile)");
    }
}
[Serializable]
public struct MaterialMeshPair
{
    public Material material;
    public Mesh mesh;
}
[Serializable]
public class MaterialMeshPairSet
{
    public MaterialMeshPair[] pairs = new MaterialMeshPair[6];
}

public struct MaterialMeshInfoSet
{
    public NativeArray<MaterialMeshInfo> pairs;
    public MaterialMeshInfoSet(int length, Allocator allocator)
    {
        pairs = new(length, allocator);
    }
}