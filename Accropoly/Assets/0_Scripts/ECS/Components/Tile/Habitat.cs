using Unity.Entities;

namespace Components
{
    [Save("Habitat")]
    public struct Habitat : IComponentData
    {
        public int totalSpace;
        public int freeSpace;
    }
}