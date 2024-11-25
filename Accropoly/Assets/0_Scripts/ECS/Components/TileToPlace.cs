using Unity.Entities;

namespace Components
{
    public struct TileToPlaceInfo : IComponentData
    {
        public TileType tileType;
        public Direction rotation;
    }
}
namespace Tags
{
    public struct TileToPlace : IComponentData { }
}