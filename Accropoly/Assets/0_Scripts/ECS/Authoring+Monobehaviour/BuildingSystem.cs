using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class BuildingSystem : MonoBehaviour
    {
        [SerializeField] private LayerMask mouseRayLayer;

        public class Baker : Baker<BuildingSystem>
        {
            public override void Bake(BuildingSystem authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new ConfigComponents.BuildingSystem
                {
                    mouseRayLayer = authoring.mouseRayLayer,
                });
            }
        }
    }
}
namespace ConfigComponents
{
    public struct BuildingSystem : IComponentData
    {
        public LayerMask mouseRayLayer;
    }
}