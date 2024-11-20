using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Components
{
    public struct BillboardOwner : IComponentData
    {
        public UnsafeList<BillboardInfo> billboards;
        public static BillboardOwner CreateInstance()
        {
            return new BillboardOwner
            {
                billboards = new(0, Allocator.Persistent)
            };
        }
    }
    public struct BillboardInfo : IComponentData
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