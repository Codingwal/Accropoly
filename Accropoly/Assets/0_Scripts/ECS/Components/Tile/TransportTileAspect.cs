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
        [Optional] private readonly RefRO<ConnectingTile> connectingTile;
        public readonly float2 TravelOnTile(Direction direction, float time, out bool reachedDest)
        {
            float2 pos = new(float.NaN);

            TileType tileType = tile.ValueRO.tileType;
            Direction rotation = tile.ValueRO.rotation;

            if (tileType == TileType.Street)
            {
                int connectionIndex = connectingTile.ValueRO.GetIndex();
                if (connectionIndex == 2 || connectionIndex == 1)
                {
                    if (direction == Directions.North)
                    {
                        pos.x = offsetFromCenter;
                        pos.y = math.lerp(-1, 1, time);
                    }
                    else
                    {
                        pos.x = -offsetFromCenter;
                        pos.y = math.lerp(1, -1, time);
                    }
                }
                else
                    Debug.LogError($"Unexpected connectionIndex {connectionIndex}");
            }
            else
                Debug.LogError($"Unexpected tileType {tileType}");

            reachedDest = time >= 1;
            if (reachedDest)
                Debug.LogWarning("Reached tile end");

            return pos + tile.ValueRO.pos * 2;
        }
        public readonly void GetRoadIndex()
        {

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