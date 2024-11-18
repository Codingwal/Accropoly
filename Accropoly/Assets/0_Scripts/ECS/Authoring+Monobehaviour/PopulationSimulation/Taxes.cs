using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class Taxes : MonoBehaviour
    {
        [SerializeField] private float taxPerHappiness;
        public class Baker : Baker<Taxes>
        {
            public override void Bake(Taxes authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new ConfigComponents.Taxes
                {
                    taxPerHappiness = authoring.taxPerHappiness
                });
            }
        }
    }
}
namespace ConfigComponents
{
    public struct Taxes : IComponentData
    {
        public float taxPerHappiness;
    }
}
