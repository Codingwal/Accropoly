using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class PrefabEntity : MonoBehaviour
    {
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private GameObject personPrefab;

        public class Baker : Baker<PrefabEntity>
        {
            public override void Bake(PrefabEntity authoring)
            {
                Debug.Assert(authoring.tilePrefab != null, "tilePrefab is null");
                Debug.Assert(authoring.personPrefab != null, "personPrefab is null"); ;

                Entity entity = GetEntity(TransformUsageFlags.None);

                ConfigComponents.PrefabEntity config = new()
                {
                    tilePrefab = GetEntity(authoring.tilePrefab, TransformUsageFlags.Dynamic),
                    personPrefab = GetEntity(authoring.personPrefab, TransformUsageFlags.Dynamic),
                };

                AddComponent(entity, config);
            }
        }
    }
}
namespace ConfigComponents
{
    public struct PrefabEntity : IComponentData
    {
        public Entity tilePrefab;
        public Entity personPrefab;
    }
}