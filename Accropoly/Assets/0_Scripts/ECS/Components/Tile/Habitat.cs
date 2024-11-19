using Unity.Entities;

namespace Components
{
    public struct Habitat : IComponentData
    {
        public int totalSpace;
        public int freeSpace;
    }
}