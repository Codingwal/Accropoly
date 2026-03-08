using Unity.Entities;

namespace Components
{
    [Save("GrowingTile")]
    public struct GrowingTile : IComponentData
    {
        public float age; // In in-game hours
    }
}