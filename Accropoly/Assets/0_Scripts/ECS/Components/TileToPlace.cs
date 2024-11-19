using Unity.Entities;

namespace Components
{
    public struct TileToPlace : IComponentData
    {
        public TileType tileType;
        public Direction rotation;
    }
}