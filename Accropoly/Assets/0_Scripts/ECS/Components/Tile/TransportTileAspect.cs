using Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Components
{
    public readonly partial struct TransportTileAspect : IAspect
    {
        public readonly RefRW<TransportTile> transportTile;
        private readonly RefRO<Tile> tile;
        [Optional] private readonly RefRO<ConnectingTile> connectingTile;
        private readonly RefRO<LocalTransform> transform;

        private const float offsetFromCenter = 0.25f;
        public const float travelSecondsPerSecond = 0.007f; // Slow down travel time. If cars would use the normal timeSpeed, they would be way too fast.
        public const float defaultVerticalOffset = 0.8f;

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

        public readonly void GetPoints(ref NativeHashMap<float3, Waypoint> waypoints)
        {
            Debug.Assert(tile.ValueRO.tileType == TileType.Street);
            Debug.Assert(connectingTile.IsValid);

            int index = connectingTile.ValueRO.GetIndex();

            // The tile is assumed to face north

            if (index == ConnectingTile.notConnected)
            {
                return;
            }
            if (index == ConnectingTile.deadEnd)
            {
                return;
            }
            if (index == ConnectingTile.straight)
            {
                // north -> south
                float3 northEntry = new(-offsetFromCenter, defaultVerticalOffset, 0.95f);
                AddWaypoint(northEntry, ref waypoints);
                float3 southExit = new(-offsetFromCenter, defaultVerticalOffset, -0.95f);
                AddWaypoint(southExit, ref waypoints, true);
                LinkWaypoints(northEntry, southExit, ref waypoints);

                // south -> north
                float3 southEntry = new(offsetFromCenter, defaultVerticalOffset, -0.95f);
                AddWaypoint(southEntry, ref waypoints);
                float3 northExit = new(offsetFromCenter, defaultVerticalOffset, 0.95f);
                AddWaypoint(northExit, ref waypoints, true);
                LinkWaypoints(southEntry, northExit, ref waypoints);

                return;
            }
            if (index == ConnectingTile.curve)
            {
                // north -> east (outer curve)
                float3 northEntry = new(-offsetFromCenter, defaultVerticalOffset, 0.95f);
                AddWaypoint(northEntry, ref waypoints);
                float3 beforeCorner = new(-offsetFromCenter, defaultVerticalOffset, offsetFromCenter);
                AddWaypoint(beforeCorner, ref waypoints);
                float3 afterCorner = new(offsetFromCenter, defaultVerticalOffset, -offsetFromCenter);
                AddWaypoint(afterCorner, ref waypoints);
                float3 eastExit = new(0.95f, defaultVerticalOffset, -offsetFromCenter);
                AddWaypoint(eastExit, ref waypoints, true);

                LinkWaypoints(northEntry, beforeCorner, ref waypoints);
                LinkWaypoints(beforeCorner, afterCorner, ref waypoints);
                LinkWaypoints(afterCorner, eastExit, ref waypoints);

                // east -> north (inner curve)
                float3 eastEntry = new(0.95f, defaultVerticalOffset, offsetFromCenter);
                AddWaypoint(eastEntry, ref waypoints);
                beforeCorner = new(0.5f, defaultVerticalOffset, offsetFromCenter);
                AddWaypoint(beforeCorner, ref waypoints);
                afterCorner = new(offsetFromCenter, defaultVerticalOffset, 0.5f);
                AddWaypoint(afterCorner, ref waypoints);
                float3 northExit = new(offsetFromCenter, defaultVerticalOffset, 0.95f);
                AddWaypoint(northExit, ref waypoints, true);

                LinkWaypoints(eastEntry, beforeCorner, ref waypoints);
                LinkWaypoints(beforeCorner, afterCorner, ref waypoints);
                LinkWaypoints(afterCorner, northExit, ref waypoints);

                return;
            }
            if (index == ConnectingTile.tJunction)
            {
                (float3 northEntry, float3 northExit) = EdgeToCenter(Directions.North, ref waypoints);
                (float3 eastEntry, float3 eastExit) = EdgeToCenter(Directions.East, ref waypoints);
                (float3 southEntry, float3 southExit) = EdgeToCenter(Directions.South, ref waypoints);

                LinkWaypoints(northEntry, eastExit, ref waypoints);
                LinkWaypoints(northEntry, southExit, ref waypoints);

                LinkWaypoints(eastEntry, northExit, ref waypoints);
                LinkWaypoints(eastEntry, southExit, ref waypoints);

                LinkWaypoints(southEntry, northExit, ref waypoints);
                LinkWaypoints(southEntry, eastExit, ref waypoints);

                return;
            }
            if (index == ConnectingTile.junction)
            {
                (float3 northEntry, float3 northExit) = EdgeToCenter(Directions.North, ref waypoints);
                (float3 eastEntry, float3 eastExit) = EdgeToCenter(Directions.East, ref waypoints);
                (float3 southEntry, float3 southExit) = EdgeToCenter(Directions.South, ref waypoints);
                (float3 westEntry, float3 westExit) = EdgeToCenter(Directions.West, ref waypoints);

                LinkWaypoints(northEntry, eastExit, ref waypoints);
                LinkWaypoints(northEntry, southExit, ref waypoints);
                LinkWaypoints(northEntry, westExit, ref waypoints);

                LinkWaypoints(eastEntry, northExit, ref waypoints);
                LinkWaypoints(eastEntry, southExit, ref waypoints);
                LinkWaypoints(eastEntry, westExit, ref waypoints);

                LinkWaypoints(southEntry, northExit, ref waypoints);
                LinkWaypoints(southEntry, eastExit, ref waypoints);
                LinkWaypoints(southEntry, westExit, ref waypoints);

                LinkWaypoints(westEntry, northExit, ref waypoints);
                LinkWaypoints(westEntry, eastExit, ref waypoints);
                LinkWaypoints(westEntry, southExit, ref waypoints);

                return;
            }

            Debug.LogError("Unhandled case");
        }
        private (float3, float3) EdgeToCenter(Direction edge, ref NativeHashMap<float3, Waypoint> waypoints)
        {
            // edge -> center
            float3 edgeEntry = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(-offsetFromCenter, defaultVerticalOffset, 0.95f));
            AddWaypoint(edgeEntry, ref waypoints);
            float3 junctionEntry = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(-offsetFromCenter, defaultVerticalOffset, 0.5f));
            AddWaypoint(junctionEntry, ref waypoints);

            LinkWaypoints(edgeEntry, junctionEntry, ref waypoints);

            // center -> edge
            float3 junctionExit = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(offsetFromCenter, defaultVerticalOffset, 0.5f));
            AddWaypoint(junctionExit, ref waypoints);
            float3 edgeExit = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(offsetFromCenter, defaultVerticalOffset, 0.95f));
            AddWaypoint(edgeExit, ref waypoints, true);

            LinkWaypoints(junctionExit, edgeExit, ref waypoints);

            return (junctionEntry, junctionExit);
        }
        private void LinkWaypoints(float3 from, float3 to, ref NativeHashMap<float3, Waypoint> waypoints)
        {
            from = ToWorldSpace(from);
            to = ToWorldSpace(to);

            Waypoint copy = waypoints[from];
            copy.AddNext(to);
            waypoints[from] = copy;

            copy = waypoints[to];
            copy.AddPrevious(from);
            waypoints[to] = copy;
        }
        private void AddWaypoint(float3 pos, ref NativeHashMap<float3, Waypoint> waypoints, bool exit = false)
        {
            pos = ToWorldSpace(pos);

            Waypoint waypoint = new(pos, 1, exit);
            waypoints.Add(waypoint.pos, waypoint);

            transportTile.ValueRW.AddWaypoint(pos);
        }
        private float3 ToWorldSpace(float3 pos)
        {
            pos = math.rotate(quaternion.EulerXYZ(0, tile.ValueRO.rotation.ToRadians(), 0), pos);
            pos += transform.ValueRO.Position;
            pos = math.round(pos * 100) / 100; // Round to precision of 0.01
            return pos;
        }
    }
}