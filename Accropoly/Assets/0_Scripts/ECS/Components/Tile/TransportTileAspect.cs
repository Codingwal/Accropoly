using System.Collections;
using System.Collections.Generic;
using Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Components
{
    public readonly partial struct TransportTileAspect : IAspect
    {
        // Disable compiler warning. TransportTile is unused but a required tag
#pragma warning disable IDE0052
        private readonly RefRO<TransportTile> transportTile;
#pragma warning restore IDE0052

        private const float offsetFromCenter = 0.25f;
        private readonly RefRO<Tile> tile;
        public readonly float2 TravelOnTile(Direction entryDirection, Direction exitDirection, float time, out bool reachedDest)
        {
            Debug.Assert(tile.ValueRO.tileType == TileType.Street);

            // Calculate the rotation needed so that entryDirection is south
            int rotation = Direction.GetRotation(entryDirection, Directions.South);

            float2 pos = GetPosOnTileIgnoreRotation(exitDirection.Rotate(rotation), time);

            // Check if the destination has been reached
            reachedDest = time >= 1;

            // Rotate position "back"
            float3 rotatedPos = math.rotate(quaternion.EulerXYZ(0, -((Direction)rotation).ToRadians(), 0), new(pos.x, 0, pos.y));

            // Debug.Log($"entry={entryDirection} -> r={rotation} => exit={exitDirection} -> normalizedExit={exitDirection.Rotate(rotation)}");

            return rotatedPos.xz + tile.ValueRO.pos * 2;
        }
        // entryDirection is always south
        private readonly float2 GetPosOnTileIgnoreRotation(Direction exitDirection, float time)
        {
            Debug.Assert(exitDirection != Directions.South);

            float2 pos = new(float.NaN); // Placeholder value, will be overwritten

            if (exitDirection == Directions.North)
            {
                pos.x = offsetFromCenter;
                pos.y = math.lerp(-1, 1, time);
            }
            else if (exitDirection == Directions.East)
            {
                pos.x = math.lerp(offsetFromCenter, 1, time);
                pos.y = math.lerp(-1, -offsetFromCenter, time);
            }
            else
            {
                pos.x = math.lerp(offsetFromCenter, -1, time);
                pos.y = math.lerp(-1, offsetFromCenter, time);
            }

            return pos;
        }
    }
}
public struct Road
{
    public float2 entrypoint;
    public Direction exitDirection;
    public float2[] points;
    public Road(float2 entrypoint, Direction exitDirection, params float2[] points)
    {
        this.entrypoint = entrypoint;
        this.exitDirection = exitDirection;
        this.points = points;
    }
}