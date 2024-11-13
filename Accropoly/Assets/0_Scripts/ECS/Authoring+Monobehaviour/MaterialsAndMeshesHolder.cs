using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class MaterialsAndMeshesHolder : MonoBehaviour
{
    private static MaterialsAndMeshesHolder instance = null;
    [SerializeField] private SerializableDictionary<TileType, MaterialMeshPair> simpleTiles = new();
    [SerializeField] private SerializableDictionary<TileType, MaterialMeshPairSet> connectingTiles = new();

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
        if (instance.simpleTiles.TryGetValue(tileType, out var pair))
        {
            Debug.Assert(pair.material != null, $"Material for tileType {tileType} is null");
            Debug.Assert(pair.mesh != null, $"Mesh for tileType {tileType} is null");
            return pair;
        }
        if (instance.connectingTiles.Contains(tileType)) Debug.LogError($"TileType {tileType} is a connectingTile, not a simpleTile");
        else Debug.LogError($"Material & Mesh for tileType {tileType} is missing");
        return default;
    }
    public static void UpdateMeshAndMaterial(Entity entity, MaterialMeshPair materialMeshPair)
    {
        entitiesToUpdate.Add(new(entity, materialMeshPair));
    }
    public static void UpdateMeshAndMaterial(Entity entity, TileType newTileType)
    {
        UpdateMeshAndMaterial(entity, GetMaterialAndMesh(newTileType));
    }
    public static void UpdateAppearence(Entity entity, TileType tileType, ConnectingTile connectingTile)
    {
        if (instance == null) Debug.LogError("Instance == null");

        if (instance.connectingTiles.TryGetValue(tileType, out var set))
        {
            int index = connectingTile.GetIndex();
            var pair = set.pairs[index];
            Direction rotation = connectingTile.GetRotation();

            // Update visuals
            Debug.Assert(pair.material != null, $"Material for tileType {tileType} is null");
            Debug.Assert(pair.mesh != null, $"Mesh for tileType {tileType} is null");
            UpdateMeshAndMaterial(entity, pair);

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            // Update rotation in custom component
            var mapTileComponent = entityManager.GetComponentData<MapTileComponent>(entity);
            mapTileComponent.rotation = rotation;
            entityManager.SetComponentData(entity, mapTileComponent);

            // Update actual rotation
            var localTransform = entityManager.GetComponentData<LocalTransform>(entity);
            localTransform.Rotation = quaternion.EulerXYZ(0, rotation.ToRadians(), 0);
            entityManager.SetComponentData(entity, localTransform);
        }
        if (instance.simpleTiles.Contains(tileType)) Debug.LogError($"TileType {tileType} is a simpleTile, not a connectingTile");
        else Debug.LogError($"Material & Mesh for tileType {tileType} is missing");
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