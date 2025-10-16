using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Components
{
    public struct TransportTile : IComponentData
    {
        public float timer; // Used by JunctionSystem
        public UnsafeList<Entity> waypoints;
        public TransportTile(int initialCapacity)
        {
            timer = 0;
            waypoints = new(initialCapacity, Allocator.Persistent);
        }
    }
}
