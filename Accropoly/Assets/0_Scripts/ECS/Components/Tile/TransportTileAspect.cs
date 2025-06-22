using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Components
{
    public readonly partial struct TransportTileAspect : IAspect
    {
        private readonly RefRO<TransportTile> transportTile;
        private readonly RefRO<Tile> tile;
        [Optional] private readonly RefRO<ConnectingTile> connectingTile;

        private const float offsetFromCenter = 0.25f;
        public const float travelSecondsPerSecond = 0.007f; // Slow down travel time. If cars would use the normal timeSpeed, they would be way too fast.
        public const float defaultVerticalOffset = 0.8f;

        public float Speed => transportTile.ValueRO.speed;

        /// <summary>
        /// Get all directions a car can travel to (from this tile)
        /// </summary>
        public readonly void GetDirections(ref NativeList<Direction> directions)
        {
            Debug.Assert(directions.IsCreated);

            if (connectingTile.IsValid)
            {
                foreach (Direction dir in Direction.GetDirections())
                {
                    if (connectingTile.ValueRO.IsConnected(dir))
                        directions.Add(dir);
                }
            }
            else
                throw new();
        }

        public readonly float3 TravelOnTile(Direction entryDirection, Direction exitDirection, float timeOnTile, out bool reachedDest)
        {
            Debug.Assert(tile.ValueRO.tileType == TileType.Street || tile.ValueRO.tileType == TileType.CityStreet || tile.ValueRO.tileType == TileType.ForestStreet);

            // Calculate the rotation needed so that entryDirection is south
            int rotation = Direction.GetRotation(entryDirection, Directions.South);

            float time = timeOnTile * transportTile.ValueRO.speed / 20 * travelSecondsPerSecond; // A tile is 20 meters big, speed is in m/s, timeOnTile in s

            // Calculate the position with south as the entry direction
            float3 pos = GetPosOnTileIgnoreRotation(exitDirection.Rotate(rotation), time);

            // Check if the destination has been reached
            reachedDest = time >= 1;

            // Rotate position "back"
            float3 rotatedPos = math.rotate(quaternion.EulerXYZ(0, -((Direction)rotation).ToRadians(), 0), pos);

            // Return the position (range -1 to 1) but add the tile position to convert to world space
            return rotatedPos + new float3(tile.ValueRO.pos.x, 0, tile.ValueRO.pos.y) * 2;
        }

        /// <remarks>entryDirection is always south</remarks>
        private readonly float3 GetPosOnTileIgnoreRotation(Direction exitDirection, float time)
        {
            Debug.Assert(exitDirection != Directions.South);

            float3 pos = new(float.NaN); // Placeholder value, will be overwritten

            if (exitDirection == Directions.North)
            {
                // move straight
                pos.x = offsetFromCenter;
                pos.z = math.lerp(-1, 1, time);
                pos.y = defaultVerticalOffset;
            }
            else if (exitDirection == Directions.East)
            {
                // move diagonal
                pos.x = math.lerp(offsetFromCenter, 1, time);
                pos.z = math.lerp(-1, -offsetFromCenter, time);
                pos.y = defaultVerticalOffset;
            }
            else
            {
                // move diagonal but add small straights close to the edge
                if (time < 0.25f)
                    pos.x = offsetFromCenter;
                else
                    pos.x = math.lerp(offsetFromCenter, -1, (time - 0.25f) / 0.75f);

                if (time > 0.75f)
                    pos.z = offsetFromCenter;
                else
                    pos.z = math.lerp(-1, offsetFromCenter, time / 0.75f);

                pos.y = defaultVerticalOffset;
            }

            return pos;
        }
    }
}