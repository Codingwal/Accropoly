using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class TileSpawnerConfigAuthoring : MonoBehaviour
{
    public static TileSpawnerConfigAuthoring Instance { get; private set; }
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private List<SerializableKeyValuePair<TileType, Mesh>> meshes;

    public class Baker : Baker<TileSpawnerConfigAuthoring>
    {
        public override void Bake(TileSpawnerConfigAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent<TilePrefab>(entity, GetEntity(authoring.tilePrefab, TransformUsageFlags.Dynamic));
        }
    }
    private void Awake()
    {
        if (Instance != null) Debug.LogError("Found multiple instances of TileSpawnerConfigAuthoring");
        Instance = this;
    }
    public static Mesh GetMesh(TileType tileType)
    {
        foreach (var pair in Instance.meshes)
            if (pair.key == tileType)
                return pair.value;
        Debug.LogError($"Mesh for tileType {tileType} is missing");
        return null;
    }
}
public struct TilePrefab : IComponentData
{
    public Entity tilePrefab;
    public static implicit operator TilePrefab(Entity entity)
    {
        return new TilePrefab { tilePrefab = entity };
    }
    public static implicit operator Entity(TilePrefab config)
    {
        return config.tilePrefab;
    }
}