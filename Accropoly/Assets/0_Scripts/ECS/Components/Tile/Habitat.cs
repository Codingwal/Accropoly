using Unity.Entities;

public struct Habitat : IComponentData
{
    public int totalSpace;
    public int freeSpace;
}