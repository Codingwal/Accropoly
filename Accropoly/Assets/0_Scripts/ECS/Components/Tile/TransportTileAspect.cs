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
        public readonly RefRO<TransportTile> transportTile;
        public readonly RefRO<LocalTransform> transform;
        public readonly RefRO<Tile> tile;
        [Optional] public readonly RefRO<ConnectingTile> connectingTile;
        public readonly Road[] GetRoads()
        {
            TileType tileType = tile.ValueRO.tileType;
            Direction rotation = tile.ValueRO.rotation;
            Road[] roads = null;

            if (connectingTile.IsValid)
            {
                int connectionIndex = connectingTile.ValueRO.GetIndex();
                roads = tileType switch
                {
                    TileType.Street => connectionIndex switch
                    {
                        1 => new Road[] // dead end
                        {
                            new(new(0.5f, -1), Directions.North, new float2(0.5f, 0.5f)),
                            new(new(-0.5f, 1), Directions.South, new float2(-0.5f, -1)),
                        },
                        2 => new Road[] // street
                        {
                            new(new(0.5f, -1), Directions.North, new float2(0.5f, 1)),
                            new(new(-0.5f, 1), Directions.South, new float2(-0.5f, -1)),
                        },
                        3 => new Road[] // curve
                        {
                            new(new(0.5f, -1), Directions.West, new float2(-1, 0.5f)),
                            new(new(-1, -0.5f), Directions.South, new float2(-0.5f, -1)),
                        },
                        _ => throw new($"Invalid connectionIndex {connectionIndex}")
                    },
                    _ => throw new("!")
                };
            }
            else throw new("!!");
            return roads;
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