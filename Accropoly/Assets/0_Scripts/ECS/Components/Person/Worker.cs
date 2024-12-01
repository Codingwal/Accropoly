using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    public struct Worker : IComponentData
    {
        public int2 employer;
        public float timeToWork;
    }
}
namespace Tags
{
    public struct Unemployed : IComponentData { }
}