using Unity.Collections;
using Unity.Entities;

namespace Components
{
    public struct BillboardOwner : IComponentData
    {
        public NativeList<BillboardInfo> billboards;
        public BillboardOwner(BillboardInfo element)
        {
            billboards = new(Allocator.Persistent)
            {
                element
            };
        }
        public struct BillboardInfo
        {
            public Entity entity;
            public Problems problem;
            public BillboardInfo(Entity entity, Problems problem)
            {
                this.entity = entity;
                this.problem = problem;
            }
            public enum Problems
            {
                NoElectricity,
            }
        }
    }
}