using Unity.Entities;
using UnityEngine;

public class TimeConfigAuthoring : MonoBehaviour
{
    [SerializeField] private float secondsPerDay;
    public class Baker : Baker<TimeConfigAuthoring>
    {
        public override void Bake(TimeConfigAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new TimeConfig
            {
                secondsPerDay = authoring.secondsPerDay
            });
        }
    }
}
public struct TimeConfig : IComponentData
{
    public float secondsPerDay;
}