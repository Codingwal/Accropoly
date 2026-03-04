using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Components
{
    [Save("TransportTile")]
    public struct TransportTile : IComponentData, ICustomSaving
    {
        public float timer; // Used by JunctionSystem
        public UnsafeList<Entity> waypoints;
        public TransportTile(int initialCapacity)
        {
            timer = 0;
            waypoints = new(initialCapacity, Allocator.Persistent);
        }

        public void Load(Deserializer deserializer)
        {
            timer = deserializer.Deserialize<float>();
            waypoints = new(10, Allocator.Persistent);
        }

        public void Save(Serializer serializer)
        {
            serializer.Serialize(timer);
            waypoints.Dispose();
        }
    }
}
