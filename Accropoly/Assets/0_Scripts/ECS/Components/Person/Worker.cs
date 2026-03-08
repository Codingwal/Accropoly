using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    [Save("Worker")]
    public struct Worker : IComponentData
    {
        public int2 employer;
        public float timeToWork; // in InGame seconds
    }
}
namespace Tags
{
    [Save("Unemployed")]
    public struct Unemployed : IComponentData { }
}