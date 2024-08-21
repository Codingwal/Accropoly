using Unity.Entities;
using Unity.Mathematics;

public struct MapTileComponent : IComponentData
{
    public float2 pos;
    public MapTileComponent(int x, int y)
    {
        pos = new float2(x, y);
    }
}
