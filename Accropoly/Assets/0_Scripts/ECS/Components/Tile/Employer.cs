using Unity.Entities;

namespace Components
{
    public struct Employer : IComponentData
    {
        public int totalSpace;
        public int freeSpace;
    }
}