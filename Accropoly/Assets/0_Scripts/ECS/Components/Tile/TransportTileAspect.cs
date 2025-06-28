using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using JunctionData = Waypoint.JunctionData;

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
            Debug.Assert(tile.ValueRO.tileType == TileType.Street || tile.ValueRO.tileType == TileType.CityStreet || tile.ValueRO.tileType == TileType.ForestStreet);
            Debug.Assert(connectingTile.IsValid);

            int index = connectingTile.ValueRO.GetIndex();

            // The tile is assumed to face north

            if (index == ConnectingTile.notConnected)
            {
                return;
            }
            if (index == ConnectingTile.deadEnd)
            {
                // north -> center
                float3 northEntry = new(-offsetFromCenter, defaultVerticalOffset, 0.95f);
                AddWaypoint(northEntry, 15, ref waypoints);
                float3 centerEntry = new(-offsetFromCenter, defaultVerticalOffset, 0);
                AddWaypoint(centerEntry, 6, ref waypoints);

                // center -> north
                float3 centerExit = new(offsetFromCenter, defaultVerticalOffset, 0);
                AddWaypoint(centerExit, 6, ref waypoints);
                float3 northExit = new(offsetFromCenter, defaultVerticalOffset, 0.95f);
                AddWaypoint(northExit, 15, ref waypoints, exit: true);

                LinkWaypoints(northEntry, centerEntry, ref waypoints);
                LinkWaypoints(centerEntry, centerExit, ref waypoints);
                LinkWaypoints(centerExit, northExit, ref waypoints);

                return;
            }
            if (index == ConnectingTile.straight)
            {
                // north -> south
                float3 northEntry = new(-offsetFromCenter, defaultVerticalOffset, 0.95f);
                AddWaypoint(northEntry, 17, ref waypoints);
                float3 southExit = new(-offsetFromCenter, defaultVerticalOffset, -0.95f);
                AddWaypoint(southExit, 17, ref waypoints, exit: true);
                LinkWaypoints(northEntry, southExit, ref waypoints);

                // south -> north
                float3 southEntry = new(offsetFromCenter, defaultVerticalOffset, -0.95f);
                AddWaypoint(southEntry, 17, ref waypoints);
                float3 northExit = new(offsetFromCenter, defaultVerticalOffset, 0.95f);
                AddWaypoint(northExit, 17, ref waypoints, exit: true);
                LinkWaypoints(southEntry, northExit, ref waypoints);

                return;
            }
            if (index == ConnectingTile.curve)
            {
                // north -> east (outer curve)
                float3 northEntry = new(-offsetFromCenter, defaultVerticalOffset, 0.95f);
                AddWaypoint(northEntry, 17, ref waypoints);
                float3 beforeCorner = new(-offsetFromCenter, defaultVerticalOffset, offsetFromCenter);
                AddWaypoint(beforeCorner, 11, ref waypoints);
                float3 afterCorner = new(offsetFromCenter, defaultVerticalOffset, -offsetFromCenter);
                AddWaypoint(afterCorner, 11, ref waypoints);
                float3 eastExit = new(0.95f, defaultVerticalOffset, -offsetFromCenter);
                AddWaypoint(eastExit, 17, ref waypoints, exit: true);

                LinkWaypoints(northEntry, beforeCorner, ref waypoints);
                LinkWaypoints(beforeCorner, afterCorner, ref waypoints);
                LinkWaypoints(afterCorner, eastExit, ref waypoints);

                // east -> north (inner curve)
                float3 eastEntry = new(0.95f, defaultVerticalOffset, offsetFromCenter);
                AddWaypoint(eastEntry, 17, ref waypoints);
                beforeCorner = new(0.5f, defaultVerticalOffset, offsetFromCenter);
                AddWaypoint(beforeCorner, 11, ref waypoints);
                afterCorner = new(offsetFromCenter, defaultVerticalOffset, 0.5f);
                AddWaypoint(afterCorner, 11, ref waypoints);
                float3 northExit = new(offsetFromCenter, defaultVerticalOffset, 0.95f);
                AddWaypoint(northExit, 17, ref waypoints, exit: true);

                LinkWaypoints(eastEntry, beforeCorner, ref waypoints);
                LinkWaypoints(beforeCorner, afterCorner, ref waypoints);
                LinkWaypoints(afterCorner, northExit, ref waypoints);

                return;
            }
            if (index == ConnectingTile.tJunction)
            {
                (float3 northEntry, float3 northExit) = EdgeToCenter(Directions.North, JunctionData.Priority, ref waypoints);
                (float3 eastEntry, float3 eastExit) = EdgeToCenter(Directions.East, JunctionData.GiveWay, ref waypoints);
                (float3 southEntry, float3 southExit) = EdgeToCenter(Directions.South, JunctionData.Priority, ref waypoints);

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
                (float3 northEntry, float3 northExit) = EdgeToCenter(Directions.North, JunctionData.Priority, ref waypoints);
                (float3 eastEntry, float3 eastExit) = EdgeToCenter(Directions.East, JunctionData.GiveWay, ref waypoints);
                (float3 southEntry, float3 southExit) = EdgeToCenter(Directions.South, JunctionData.Priority, ref waypoints);
                (float3 westEntry, float3 westExit) = EdgeToCenter(Directions.West, JunctionData.GiveWay, ref waypoints);

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
        private (float3, float3) EdgeToCenter(Direction edge, JunctionData junctionData, ref NativeHashMap<float3, Waypoint> waypoints)
        {
            // edge -> center
            float3 edgeEntry = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(-offsetFromCenter, defaultVerticalOffset, 0.95f));
            AddWaypoint(edgeEntry, 17, ref waypoints);
            float3 junctionEntry = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(-offsetFromCenter, defaultVerticalOffset, 0.5f));
            AddWaypoint(junctionEntry, 8, ref waypoints, junctionData: junctionData);

            LinkWaypoints(edgeEntry, junctionEntry, ref waypoints);

            // center -> edge
            float3 junctionExit = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(offsetFromCenter, defaultVerticalOffset, 0.5f));
            AddWaypoint(junctionExit, 11, ref waypoints);
            float3 edgeExit = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(offsetFromCenter, defaultVerticalOffset, 0.95f));
            AddWaypoint(edgeExit, 17, ref waypoints, exit: true);

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
        private void AddWaypoint(float3 pos, float velocity, ref NativeHashMap<float3, Waypoint> waypoints, JunctionData junctionData = JunctionData.None, bool exit = false)
        {
            pos = ToWorldSpace(pos);

            Waypoint waypoint = new(pos, velocity, junctionData, exit);
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