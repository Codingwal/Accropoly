using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    [Save]
    public struct Person : IComponentData
    {
        public int2 homeTile;
        public float happiness;
        public int age;
    }
}
namespace Tags
{
    public struct Homeless : IComponentData { }

    public struct FreeTime : IComponentData { }
    public struct Resting : IComponentData { }
    public struct Working : IComponentData { }
}