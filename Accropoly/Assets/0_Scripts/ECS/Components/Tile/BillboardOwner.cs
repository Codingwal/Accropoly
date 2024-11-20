using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Components
{
    public struct BillboardOwner : IComponentData
    {
        public UnsafeList<BillboardInfo> billboards;
        public BillboardOwner(BillboardInfo element)
        {
            billboards = new(1, Allocator.Persistent)
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