using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    public struct Worker : IComponentData
    {
        public int2 employer;
        public float timeToWork; // in InGame seconds
    }
}
namespace Tags
{
    public struct Unemployed : IComponentData { }
}