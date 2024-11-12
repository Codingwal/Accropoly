using Unity.Entities;
using Unity.Mathematics;

public struct PersonComponent : IComponentData
{
    public int2 homeTile;
    public float happiness;
    public int age;
}
public struct HomelessTag : IComponentData { }
