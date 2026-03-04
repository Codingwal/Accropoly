using Unity.Entities;

namespace Components
{
    [Save("Employer")]
    public struct Employer : IComponentData
    {
        public int totalSpace;
        public int freeSpace;
    }
}