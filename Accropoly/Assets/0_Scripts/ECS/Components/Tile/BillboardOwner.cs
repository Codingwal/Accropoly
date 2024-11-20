using Unity.Entities;

namespace Components
{
    public struct BillboardOwner : IComponentData
    {
        public Entity billboardEntity;
    }
}