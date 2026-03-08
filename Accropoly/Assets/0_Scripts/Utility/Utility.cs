using Unity.Mathematics;

public static class Utility
{
    public static float3 TileToWaypoint(int2 tilePos)
    {
        return new float3(2 * tilePos.x, 0.8f, 2 * tilePos.y);
    }
}