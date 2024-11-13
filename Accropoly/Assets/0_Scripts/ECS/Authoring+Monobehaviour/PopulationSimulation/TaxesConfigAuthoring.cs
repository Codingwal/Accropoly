using Unity.Entities;
using UnityEngine;

public class TaxesConfigAuthoring : MonoBehaviour
{
    [SerializeField] private float taxPerHappiness;
    public class Baker : Baker<TaxesConfigAuthoring>
    {
        public override void Bake(TaxesConfigAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new TaxesConfig
            {
                taxPerHappiness = authoring.taxPerHappiness
            });
        }
    }
}
public struct TaxesConfig : IComponentData
{
    public float taxPerHappiness;
}
