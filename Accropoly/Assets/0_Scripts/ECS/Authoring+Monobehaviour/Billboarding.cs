using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class Billboarding : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;

        public class Baker : Baker<Billboarding>
        {
            public override void Bake(Billboarding authoring)
            {
                Debug.Assert(authoring.prefab != null, "tilePrefab is null");

                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent<ConfigComponents.BillboardPrefab>(entity, GetEntity(authoring.prefab, TransformUsageFlags.Dynamic));
            }
        }
    }
}
namespace ConfigComponents
{
    public struct BillboardPrefab : IComponentData
    {
        public Entity prefab;
        public static implicit operator BillboardPrefab(Entity entity)
        {
            return new BillboardPrefab { prefab = entity };
        }
        public static implicit operator Entity(BillboardPrefab config)
        {
            return config.prefab;
        }
    }
}