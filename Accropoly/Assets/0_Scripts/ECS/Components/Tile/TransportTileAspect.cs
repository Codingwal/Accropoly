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

        public readonly void GetPoints(ref NativeList<Waypoint> waypoints, ref NativeList<WaypointSystem.Connection> connections)
        {
            Debug.Assert(tile.ValueRO.tileType == TileType.Street);
            Debug.Assert(connectingTile.IsValid);

            int index = connectingTile.ValueRO.GetIndex();

            // The tile is assumed to face south

            int waypointIndex;
            if (index == ConnectingTile.straight)
            {
                // south -> north
                waypointIndex = AddWaypoint(new(offsetFromCenter, defaultVerticalOffset, 0), ref waypoints);
                AddConnection(new(offsetFromCenter, defaultVerticalOffset, 1), waypointIndex, true, ref connections);
                AddConnection(new(offsetFromCenter, defaultVerticalOffset, -1), waypointIndex, false, ref connections);

                // north -> south
                waypointIndex = AddWaypoint(new(-offsetFromCenter, defaultVerticalOffset, 0), ref waypoints);
                AddConnection(new(-offsetFromCenter, defaultVerticalOffset, -1), waypointIndex, true, ref connections);
                AddConnection(new(-offsetFromCenter, defaultVerticalOffset, 1), waypointIndex, false, ref connections);

            }
            else if (index == ConnectingTile.curve)
            {
                // south -> west
                waypointIndex = AddWaypoint(new(offsetFromCenter, defaultVerticalOffset, offsetFromCenter), ref waypoints);
                AddConnection(new(-1, defaultVerticalOffset, offsetFromCenter), waypointIndex, true, ref connections);
                AddConnection(new(offsetFromCenter, defaultVerticalOffset, -1), waypointIndex, false, ref connections);

                // west -> south
                waypointIndex = AddWaypoint(new(-offsetFromCenter, defaultVerticalOffset, -offsetFromCenter), ref waypoints);
                AddConnection(new(-offsetFromCenter, defaultVerticalOffset, -1), waypointIndex, true, ref connections);
                AddConnection(new(-1, defaultVerticalOffset, -offsetFromCenter), waypointIndex, false, ref connections);
            }
        }
        private int AddWaypoint(float3 pos, ref NativeList<Waypoint> waypoints)
        {
            Debug.Log($"rotation: {tile.ValueRO.rotation}");
            pos = math.rotate(quaternion.EulerXYZ(0, tile.ValueRO.rotation.Flip().ToRadians(), 0), pos); // Rotate
            pos += transform.ValueRO.Position; // Convert to world space
            Waypoint waypoint = new(pos);
            waypoints.Add(waypoint);
            return waypoints.Length - 1;
        }
        private void AddConnection(float3 pos, int connectedWaypoint, bool output, ref NativeList<WaypointSystem.Connection> connections)
        {
            // Debug.Log($"rotation: {tile.ValueRO.rotation}");
            pos = math.rotate(quaternion.EulerXYZ(0, tile.ValueRO.rotation.Flip().ToRadians(), 0), pos); // Rotate
            pos += transform.ValueRO.Position; // Convert to world space
            WaypointSystem.Connection connection = new(pos, connectedWaypoint, output);
            connections.Add(connection);
        }

    }
}