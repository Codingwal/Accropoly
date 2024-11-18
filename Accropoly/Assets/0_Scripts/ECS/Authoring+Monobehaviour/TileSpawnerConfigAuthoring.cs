using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class PrefabEntity : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;

        public class Baker : Baker<PrefabEntity>
        {
            public override void Bake(PrefabEntity authoring)
            {
                Debug.Assert(authoring.prefab != null, "tilePrefab is null");

                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent<ConfigComponents.PrefabEntity>(entity, GetEntity(authoring.prefab, TransformUsageFlags.Dynamic));
            }
        }
    }
}
namespace ConfigComponents
{
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
}