using Unity.Entities;
using UnityEngine;

public class TileSpawnerConfigAuthoring : MonoBehaviour
{
    [SerializeField] GameObject tilePrefab;

    public class Baker : Baker<TileSpawnerConfigAuthoring>
    {
        public override void Bake(TileSpawnerConfigAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new TileSpawnerConfig
            {
                tilePrefab = GetEntity(authoring.tilePrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}
public struct TileSpawnerConfig : IComponentData
{
    public Entity tilePrefab;
}
