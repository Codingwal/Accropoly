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

            AddComponent<PrefabEntity>(entity, GetEntity(authoring.prefab, TransformUsageFlags.Dynamic));
        }
    }
}
public struct PrefabEntity : IComponentData
{
    public Entity prefab;
    public static implicit operator PrefabEntity(Entity entity)
    {
        return new PrefabEntity { prefab = entity };
    }
    public static implicit operator Entity(PrefabEntity config)
    {
        return config.prefab;
    }
}