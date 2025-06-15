using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Authoring
{
    public class TileGrowing : MonoBehaviour
    {
        [Header("In hours")]
        [SerializeField] private float maxAge1;
        [SerializeField] private float maxAge2;
        public class Baker : Baker<TileGrowing>
        {
            public override void Bake(TileGrowing authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new ConfigComponents.TileGrowing
                {
                    // Convert from hours to seconds
                    maxAge1 = (int)(authoring.maxAge1 * 3600),
                    maxAge2 = (int)(authoring.maxAge2 * 3600),
                });
            }
        }
    }
}
namespace ConfigComponents
{
    public struct TileGrowing : IComponentData
    {
        // In in-game seconds
        public int maxAge1;
        public int maxAge2;
    }
}