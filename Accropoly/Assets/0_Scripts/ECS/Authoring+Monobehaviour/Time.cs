using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class Time : MonoBehaviour
    {
        [SerializeField] private float secondsPerDay;
        public class Baker : Baker<Time>
        {
            public override void Bake(Time authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new ConfigComponents.Time
                {
                    secondsPerDay = authoring.secondsPerDay
                });
            }
        }
    }
}
namespace ConfigComponents
{
    public struct Time : IComponentData
    {
        public float secondsPerDay;
    }
}