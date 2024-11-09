using Unity.Entities;
using Unity.Mathematics;

public struct Worker : IComponentData
{
    public int2 employer;
}

