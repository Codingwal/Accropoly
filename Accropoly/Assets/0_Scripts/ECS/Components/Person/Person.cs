using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    [Save("Person")]
    public struct Person : IComponentData
    {
        public int2 homeTile;
        public float happiness;
        public int age;
    }
}
namespace Tags
{
    [Save("Homeless")]
    public struct Homeless : IComponentData { }

    [Save("FreeTime")]
    public struct FreeTime : IComponentData { }

    [Save("Resting")]
    public struct Resting : IComponentData { }

    [Save("Working")]
    public struct Working : IComponentData { }
}