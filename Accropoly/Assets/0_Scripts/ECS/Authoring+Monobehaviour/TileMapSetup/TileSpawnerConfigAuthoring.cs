using Unity.Entities;
using UnityEngine;

public class TileSpawnerConfigAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;

    public class Baker : Baker<TileSpawnerConfigAuthoring>
    {
        public override void Bake(TileSpawnerConfigAuthoring authoring)
        {
            Debug.Assert(authoring.tilePrefab != null, "tilePrefab is null");

            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent<TilePrefab>(entity, GetEntity(authoring.tilePrefab, TransformUsageFlags.Dynamic));
        }
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
[System.Serializable]
public struct MaterialMeshPair
{
    public Material material;
    public Mesh mesh;
}