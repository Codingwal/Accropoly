using Tags;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Components
{
    public readonly partial struct TransportTileAspect : IAspect
    {
        private readonly RefRO<TransportTile> transportTile;

        private const float offsetFromCenter = 0.25f;
        public const float travelSecondsPerSecond = 0.007f; // Slow down travel time. If cars would use the normal timeSpeed, they would be way too fast.

        private readonly RefRO<Tile> tile;
        public readonly float2 TravelOnTile(Direction entryDirection, Direction exitDirection, float timeOnTile, out bool reachedDest)
        {
            Debug.Assert(tile.ValueRO.tileType == TileType.Street);

            // Calculate the rotation needed so that entryDirection is south
            int rotation = Direction.GetRotation(entryDirection, Directions.South);

            float time = timeOnTile * transportTile.ValueRO.speed / 20 * travelSecondsPerSecond; // A tile is 20 meters big, speed is in m/s, timeOnTile in s

            // Calculate the position with south as the entry direction
            float2 pos = GetPosOnTileIgnoreRotation(exitDirection.Rotate(rotation), time);

            // Check if the destination has been reached
            reachedDest = time >= 1;

            // Rotate position "back"
            float3 rotatedPos = math.rotate(quaternion.EulerXYZ(0, -((Direction)rotation).ToRadians(), 0), new(pos.x, 0, pos.y));

            // Return the position (range -1 to 1) but add the tile position to convert to world space
            return rotatedPos.xz + tile.ValueRO.pos * 2;
        }

        /// <remarks>entryDirection is always south</remarks>
        private readonly float2 GetPosOnTileIgnoreRotation(Direction exitDirection, float time)
        {
            Debug.Assert(exitDirection != Directions.South);

            float2 pos = new(float.NaN); // Placeholder value, will be overwritten

            if (exitDirection == Directions.North)
            {
                // move straight
                pos.x = offsetFromCenter;
                pos.y = math.lerp(-1, 1, time);
            }
            else if (exitDirection == Directions.East)
            {
                // move diagonal
                pos.x = math.lerp(offsetFromCenter, 1, time);
                pos.y = math.lerp(-1, -offsetFromCenter, time);
            }
            else
            {
                // move diagonal but add small straights close to the edge
                if (time < 0.25f)
                    pos.x = offsetFromCenter;
                else
                    pos.x = math.lerp(offsetFromCenter, -1, (time - 0.25f) / 0.75f);

                if (time > 0.75f)
                    pos.y = offsetFromCenter;
                else
                    pos.y = math.lerp(-1, offsetFromCenter, time / 0.75f);
            }

            return pos;
        }
    }
}