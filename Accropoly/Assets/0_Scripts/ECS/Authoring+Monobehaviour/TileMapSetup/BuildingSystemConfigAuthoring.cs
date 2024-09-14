using Unity.Entities;
using UnityEngine;

public class BuildingSystemConfigAuthoring : MonoBehaviour
{
    [SerializeField] private LayerMask mouseRayLayer;

    public class Baker : Baker<BuildingSystemConfigAuthoring>
    {
        public override void Bake(BuildingSystemConfigAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new BuildingSystemConfig
            {
                mouseRayLayer = authoring.mouseRayLayer,
            });
        }
    }
}
public struct BuildingSystemConfig : IComponentData
{
    public LayerMask mouseRayLayer;
}