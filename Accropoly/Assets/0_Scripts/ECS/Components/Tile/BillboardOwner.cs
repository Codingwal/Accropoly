using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Components
{
    [Save("BillboardOwner")]
    public struct BillboardOwner : IComponentData, ICustomSaving
    {
        public UnsafeList<BillboardInfo> billboards;
        public readonly bool IsInitialized => billboards.IsCreated;
        public void Initialize()
        {
            billboards = new(2, Allocator.Persistent);
        }

        public void Load(Deserializer deserializer)
        {

        }

        public void Save(Serializer serializer)
        {

        }
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
            NotConnected,
        }
    }
}