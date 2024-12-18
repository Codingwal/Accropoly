using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Authoring
{
    public class TileAgeing : MonoBehaviour
    {
        [SerializeField] private uint seed;
        [SerializeField] private int2 randomAgeRange;
        [SerializeField] private int maxAge;
        [SerializeField] private TileType newTileType;
        public class Baker : Baker<TileAgeing>
        {
            public override void Bake(TileAgeing authoring)
            {
                Debug.Assert(authoring.seed != 0, "Seed should not be 0");
                Debug.Assert(authoring.randomAgeRange.x < authoring.randomAgeRange.y, "First parameter of randomAgeRange is the min value");

                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new ConfigComponents.TileAgeing
                {
                    seed = authoring.seed,
                    randomAgeRange = authoring.randomAgeRange,
                    maxAge = authoring.maxAge,
                    newTileType = authoring.newTileType
                });
            }
        }
    }
}
namespace ConfigComponents
{
    public struct TileAgeing : IComponentData
    {
        public uint seed;
        public int2 randomAgeRange;
        public float maxAge;
        public TileType newTileType;
    }
}