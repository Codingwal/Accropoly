using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    public struct Worker : IComponentData
    {
        public int2 employer;
    }
}
namespace Tags
{
    public struct Unemployed : IComponentData { }
}