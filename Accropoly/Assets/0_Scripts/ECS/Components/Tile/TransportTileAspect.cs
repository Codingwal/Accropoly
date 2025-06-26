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

        public readonly void GetPoints(ref NativeHashMap<float3, Waypoint> waypoints, ref NativeList<WaypointSystem.Connection> connections)
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
                float3 waypointPos = new(-offsetFromCenter, defaultVerticalOffset, 0);
                AddWaypoint(waypointPos, ref waypoints);
                AddConnection(new(-offsetFromCenter, defaultVerticalOffset, -1), waypointPos, true, ref connections); // south exit
                AddConnection(new(-offsetFromCenter, defaultVerticalOffset, 1), waypointPos, false, ref connections); // north entry

                // south -> north
                waypointPos = new(offsetFromCenter, defaultVerticalOffset, 0);
                AddWaypoint(waypointPos, ref waypoints);
                AddConnection(new(offsetFromCenter, defaultVerticalOffset, 1), waypointPos, true, ref connections); // north exit
                AddConnection(new(offsetFromCenter, defaultVerticalOffset, -1), waypointPos, false, ref connections); // south entry

                return;
            }
            if (index == ConnectingTile.curve)
            {
                // north -> east (outer curve)

                float3 beforeCorner = new(-offsetFromCenter, defaultVerticalOffset, 0);
                AddWaypoint(beforeCorner, ref waypoints);
                float3 afterCorner = new(0, defaultVerticalOffset, -offsetFromCenter);
                AddWaypoint(afterCorner, ref waypoints);

                LinkWaypoints(beforeCorner, afterCorner, ref waypoints);

                AddConnection(new(-offsetFromCenter, defaultVerticalOffset, 1), beforeCorner, false, ref connections); // north entry
                AddConnection(new(1, defaultVerticalOffset, -offsetFromCenter), afterCorner, true, ref connections); // east exit

                // east -> north (inner curve)

                beforeCorner = new(0.5f, defaultVerticalOffset, offsetFromCenter);
                AddWaypoint(beforeCorner, ref waypoints);
                afterCorner = new(offsetFromCenter, defaultVerticalOffset, 0.5f);
                AddWaypoint(afterCorner, ref waypoints);

                LinkWaypoints(beforeCorner, afterCorner, ref waypoints);

                AddConnection(new(1, defaultVerticalOffset, offsetFromCenter), beforeCorner, false, ref connections); // north entry
                AddConnection(new(offsetFromCenter, defaultVerticalOffset, 1), afterCorner, true, ref connections); // east exit

                return;
            }
            if (index == ConnectingTile.tJunction)
            {
                (float3 northEntry, float3 northExit) = EdgeToCenter(Directions.North, ref waypoints, ref connections);
                (float3 eastEntry, float3 eastExit) = EdgeToCenter(Directions.East, ref waypoints, ref connections);
                (float3 southEntry, float3 southExit) = EdgeToCenter(Directions.South, ref waypoints, ref connections);

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
                (float3 northEntry, float3 northExit) = EdgeToCenter(Directions.North, ref waypoints, ref connections);
                (float3 eastEntry, float3 eastExit) = EdgeToCenter(Directions.East, ref waypoints, ref connections);
                (float3 southEntry, float3 southExit) = EdgeToCenter(Directions.South, ref waypoints, ref connections);
                (float3 westEntry, float3 westExit) = EdgeToCenter(Directions.West, ref waypoints, ref connections);

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
        public readonly void GetConnections(Direction edge, ref NativeList<WaypointSystem.Connection> connections)
        {
            int index = connectingTile.ValueRO.GetIndex();

            if (index == ConnectingTile.straight)
            {
                float3 waypointPos = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(-offsetFromCenter, defaultVerticalOffset, 0));
                Debug.Assert(transportTile.ValueRO.waypoints.Contains(waypointPos));
                AddConnection(new(-offsetFromCenter, defaultVerticalOffset, 1), waypointPos, false, ref connections); // north entry

                waypointPos = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(offsetFromCenter, defaultVerticalOffset, 0));
                Debug.Assert(transportTile.ValueRO.waypoints.Contains(waypointPos));
                AddConnection(new(offsetFromCenter, defaultVerticalOffset, 1), waypointPos, true, ref connections); // north exit
            }
            if (index == ConnectingTile.curve)
            {
            }
            if (index == ConnectingTile.tJunction || index == ConnectingTile.junction)
            {
                // edge -> center
                float3 junctionEntry = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(-offsetFromCenter, defaultVerticalOffset, 0.5f));
                Debug.Assert(transportTile.ValueRO.waypoints.Contains(junctionEntry));
                float3 tmp = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(-offsetFromCenter, defaultVerticalOffset, 1));
                AddConnection(tmp, junctionEntry, false, ref connections);

                // center -> edge
                float3 junctionExit = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(offsetFromCenter, defaultVerticalOffset, 0.5f));
                Debug.Assert(transportTile.ValueRO.waypoints.Contains(junctionExit));
                tmp = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(offsetFromCenter, defaultVerticalOffset, 1));
                AddConnection(tmp, junctionExit, true, ref connections);

                return;
            }
        }

        private (float3, float3) EdgeToCenter(Direction edge, ref NativeHashMap<float3, Waypoint> waypoints, ref NativeList<WaypointSystem.Connection> connections)
        {
            // edge -> center
            float3 junctionEntry = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(-offsetFromCenter, defaultVerticalOffset, 0.5f));
            AddWaypoint(junctionEntry, ref waypoints);
            float3 tmp = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(-offsetFromCenter, defaultVerticalOffset, 1));
            AddConnection(tmp, junctionEntry, false, ref connections);

            // center -> edge
            float3 junctionExit = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(offsetFromCenter, defaultVerticalOffset, 0.5f));
            AddWaypoint(junctionExit, ref waypoints);
            tmp = math.rotate(quaternion.EulerXYZ(0, edge.ToRadians(), 0), new(offsetFromCenter, defaultVerticalOffset, 1));
            AddConnection(tmp, junctionExit, true, ref connections);

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
        private void AddWaypoint(float3 pos, ref NativeHashMap<float3, Waypoint> waypoints)
        {
            pos = ToWorldSpace(pos);

            Waypoint waypoint = new(pos);
            waypoints.Add(waypoint.pos, waypoint);

            transportTile.ValueRW.AddWaypoint(pos);
        }
        private void AddConnection(float3 pos, float3 connectedWaypoint, bool output, ref NativeList<WaypointSystem.Connection> connections)
        {
            pos = ToWorldSpace(pos);
            connectedWaypoint = ToWorldSpace(connectedWaypoint);
            WaypointSystem.Connection connection = new(pos, connectedWaypoint, output);
            connections.Add(connection);
            transportTile.ValueRW.AddConnection(pos);
        }
        private float3 ToWorldSpace(float3 pos)
        {
            pos = math.rotate(quaternion.EulerXYZ(0, tile.ValueRO.rotation.ToRadians(), 0), pos);
            pos += transform.ValueRO.Position;
            pos = math.round(pos * 100) / 100; // Round to precision of 0.01
            return pos;
        }
        private float3 FromWorldSpace(float3 pos)
        {
            pos -= transform.ValueRO.Position;
            pos = math.rotate(quaternion.EulerXYZ(0, -tile.ValueRO.rotation.ToRadians(), 0), pos);
            pos = math.round(pos * 100) / 100; // Round to precision of 0.01
            return pos;
        }
    }
}