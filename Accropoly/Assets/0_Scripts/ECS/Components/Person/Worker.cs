using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    public struct Worker : IComponentData
    {
        public int2 employer;
    }
}
public struct UnemployedTag : IComponentData { }
