using Unity.Entities;
using Unity.Mathematics;

public struct PersonComponent : IComponentData
{
    public int2 homeTile;
    public int age;
}
public struct HomelessTag : IComponentData { }
