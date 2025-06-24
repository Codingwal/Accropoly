using Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Components
{
    public readonly partial struct TransportTileAspect : IAspect
    {
        private readonly RefRW<TransportTile> transportTile;
        private readonly RefRO<Tile> tile;
        [Optional] private readonly RefRO<ConnectingTile> connectingTile;
        private readonly RefRO<LocalTransform> transform;

        private const float offsetFromCenter = 0.25f;
        public const float travelSecondsPerSecond = 0.007f; // Slow down travel time. If cars would use the normal timeSpeed, they would be way too fast.
        public const float defaultVerticalOffset = 0.8f;

        public float Speed => transportTile.ValueRO.speed;
        public TransportTile TransportTile => transportTile.ValueRO;

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

        public readonly void GetPoints(ref NativeList<Waypoint> waypoints, ref NativeList<WaypointSystem.Connection> connections)
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
                // north -> center
                int w1 = AddWaypoint(new(-offsetFromCenter, defaultVerticalOffset, 0), ref waypoints);
                AddConnection(new(-offsetFromCenter, defaultVerticalOffset, 1), w1, false, ref connections); // north entry

                // center -> north
                int w2 = AddWaypoint(new(offsetFromCenter, defaultVerticalOffset, 0), ref waypoints);
                AddConnection(new(offsetFromCenter, defaultVerticalOffset, 1), w2, true, ref connections); // north exit

                LinkWaypoints(w1, w2, ref waypoints);

                return;
            }
            if (index == ConnectingTile.straight)
            {
                // north -> south
                int w1 = AddWaypoint(new(-offsetFromCenter, defaultVerticalOffset, 0), ref waypoints);
                AddConnection(new(-offsetFromCenter, defaultVerticalOffset, -1), w1, true, ref connections); // south exit
                AddConnection(new(-offsetFromCenter, defaultVerticalOffset, 1), w1, false, ref connections); // north entry

                // south -> north
                int w2 = AddWaypoint(new(offsetFromCenter, defaultVerticalOffset, 0), ref waypoints);
                AddConnection(new(offsetFromCenter, defaultVerticalOffset, 1), w2, true, ref connections); // north exit
                AddConnection(new(offsetFromCenter, defaultVerticalOffset, -1), w2, false, ref connections); // south entry
                return;
            }
            if (index == ConnectingTile.curve)
            {
                // north -> east (outer curve)

                int w1 = AddWaypoint(new(-offsetFromCenter, defaultVerticalOffset, 0), ref waypoints); // Before corner
                int w2 = AddWaypoint(new(0, defaultVerticalOffset, -offsetFromCenter), ref waypoints); // After corner

                LinkWaypoints(w1, w2, ref waypoints);

                AddConnection(new(-offsetFromCenter, defaultVerticalOffset, 1), w1, false, ref connections); // north entry
                AddConnection(new(1, defaultVerticalOffset, -offsetFromCenter), w2, true, ref connections); // east exit

                // east -> north (inner curve)

                int w3 = AddWaypoint(new(0.5f, defaultVerticalOffset, offsetFromCenter), ref waypoints); // Before corner
                int w4 = AddWaypoint(new(offsetFromCenter, defaultVerticalOffset, 0.5f), ref waypoints); // After corner

                LinkWaypoints(w3, w4, ref waypoints);

                AddConnection(new(1, defaultVerticalOffset, offsetFromCenter), w3, false, ref connections); // east entry
                AddConnection(new(offsetFromCenter, defaultVerticalOffset, 1), w4, true, ref connections); // north exit
                return;
            }
            if (index == ConnectingTile.tJunction)
            {
                (int northEntry, int northExit) = EdgeToCenter(Directions.North, ref waypoints, ref connections);
                (int eastEntry, int eastExit) = EdgeToCenter(Directions.East, ref waypoints, ref connections);
                (int southEntry, int southExit) = EdgeToCenter(Directions.South, ref waypoints, ref connections);

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
                (int northEntry, int northExit) = EdgeToCenter(Directions.North, ref waypoints, ref connections);
                (int eastEntry, int eastExit) = EdgeToCenter(Directions.East, ref waypoints, ref connections);
                (int southEntry, int southExit) = EdgeToCenter(Directions.South, ref waypoints, ref connections);
                (int westEntry, int westExit) = EdgeToCenter(Directions.West, ref waypoints, ref connections);

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
        private (int, int) EdgeToCenter(Direction edge, ref NativeList<Waypoint> waypoints, ref NativeList<WaypointSystem.Connection> connections)
        {
            // north -> center
            float3 pos = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(-offsetFromCenter, defaultVerticalOffset, 0.5f));
            int junctionEntry = AddWaypoint(pos, ref waypoints);
            pos = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(-offsetFromCenter, defaultVerticalOffset, 1));
            AddConnection(pos, junctionEntry, false, ref connections);

            // center -> north
            pos = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(offsetFromCenter, defaultVerticalOffset, 0.5f));
            int junctionExit = AddWaypoint(pos, ref waypoints);
            pos = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(offsetFromCenter, defaultVerticalOffset, 1));
            AddConnection(pos, junctionExit, true, ref connections);

            return (junctionEntry, junctionExit);
        }
        private void LinkWaypoints(int from, int to, ref NativeList<Waypoint> waypoints)
        {
            waypoints.ElementAt(from).AddNext(to);
            waypoints.ElementAt(to).AddPrevious(from);
        }
        private int AddWaypoint(float3 pos, ref NativeList<Waypoint> waypoints)
        {
            pos = math.rotate(quaternion.EulerXYZ(0, tile.ValueRO.rotation.ToRadians(), 0), pos); // Rotate
            pos += transform.ValueRO.Position; // Convert to world space

            Waypoint waypoint = new(pos);
            waypoints.Add(waypoint);
            int waypointIndex = waypoints.Length - 1;

            transportTile.ValueRW.AddWaypoint(waypointIndex);

            return waypointIndex;
        }
        private void AddConnection(float3 pos, int connectedWaypoint, bool output, ref NativeList<WaypointSystem.Connection> connections)
        {
            pos = math.rotate(quaternion.EulerXYZ(0, tile.ValueRO.rotation.ToRadians(), 0), pos); // Rotate
            pos += transform.ValueRO.Position; // Convert to world space
            WaypointSystem.Connection connection = new(pos, connectedWaypoint, output);
            connections.Add(connection);
            transportTile.ValueRW.AddConnection(pos);
        }

    }
}