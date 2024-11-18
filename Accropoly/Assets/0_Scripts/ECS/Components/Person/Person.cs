using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
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
}