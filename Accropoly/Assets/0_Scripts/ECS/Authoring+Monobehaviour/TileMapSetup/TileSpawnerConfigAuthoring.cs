using Unity.Entities;
using UnityEngine;

public class TileSpawnerConfigAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject prefab;

    public class Baker : Baker<TileSpawnerConfigAuthoring>
    {
        public override void Bake(TileSpawnerConfigAuthoring authoring)
        {
            Debug.Assert(authoring.prefab != null, "tilePrefab is null");

            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent<Prefab>(entity, GetEntity(authoring.prefab, TransformUsageFlags.Dynamic));
        }
    }
}
public struct Prefab : IComponentData
{
    public Entity tilePrefab;
    public static implicit operator Prefab(Entity entity)
    {
        return new Prefab { tilePrefab = entity };
    }
    public static implicit operator Entity(Prefab config)
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