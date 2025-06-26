using Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class WaypointSystem : SystemBase
    {
        public static NativeHashMap<float3, Waypoint> waypoints;
        public static NativeHashMap<float3, Connection> connectionPoints;
        protected override void OnCreate()
        {
            waypoints = new(30, Allocator.Persistent);
            connectionPoints = new(30, Allocator.Persistent);
        }
        protected override void OnUpdate()
        {
            NativeList<Connection> connectionsTmp = new(4, Allocator.TempJob);
            Entities.WithChangeFilter<ConnectingTile>().ForEach((TransportTileAspect transportTileAspect, in Tile tile) =>
            {
                // Delete all waypoints and connections owned by this tile
                unsafe
                {
                    for (int i = 0; i < transportTileAspect.TransportTile.waypoints.Size; i++)
                    {
                        float3 waypoint = transportTileAspect.TransportTile.waypoints[i];
                        if (!waypoints.ContainsKey(waypoint)) continue;
                        DeleteWaypoint(waypoint, ref waypoints);
                    }
                    for (int i = 0; i < transportTileAspect.TransportTile.connections.Size; i++)
                    {
                        float3 connection = transportTileAspect.TransportTile.connections[i];
                        if (!connectionPoints.ContainsKey(connection)) continue;
                        connectionPoints.Remove(connection);
                    }
                }

                transportTileAspect.GetPoints(ref waypoints, ref connectionsTmp);

                foreach (var connection in connectionsTmp)
                {
                    if (!connectionPoints.ContainsKey(connection.pos))
                    {
                        connectionPoints.Add(connection.pos, connection);
                    }
                    else
                    {
                        Connection other = connectionPoints[connection.pos];
                        connectionPoints.Remove(other.pos); // Remove from connection points as the connection will now be made
                        Debug.Assert(connection.output != other.output); // They can't both be input/output

                        // Connect tiles
                        if (connection.output) // this -> other
                        {
                            // All of this copying is needed as AddNext modifies the data

                            Waypoint copy = waypoints[connection.waypoint];
                            copy.AddNext(other.waypoint);
                            waypoints[connection.waypoint] = copy;

                            copy = waypoints[other.waypoint];
                            copy.AddPrevious(connection.waypoint);
                            waypoints[other.waypoint] = copy;
                        }
                        else // other -> this
                        {
                            // All of this copying is needed as AddNext modifies the data

                            Waypoint copy = waypoints[other.waypoint];
                            copy.AddNext(connection.waypoint);
                            waypoints[other.waypoint] = copy;

                            copy = waypoints[connection.waypoint];
                            copy.AddPrevious(other.waypoint);
                            waypoints[connection.waypoint] = copy;
                        }
                    }
                }

                connectionsTmp.Clear();
            }).WithDisposeOnCompletion(connectionsTmp).WithoutBurst().Schedule();
        }
        protected override void OnDestroy()
        {
            waypoints.Dispose();
            connectionPoints.Dispose();
        }

        public void DrawGizmos()
        {
            // Draw waypoints and connections inbetween
            Gizmos.color = Color.blue;
            foreach (var pair in waypoints)
            {
                Waypoint waypoint = pair.Value;
                Gizmos.DrawSphere(waypoint.pos, 0.15f);

                unsafe
                {
                    for (int i = 0; i < waypoint.next.Size; i++)
                    {
                        float3 nextPos = waypoint.next[i];
                        if (!math.isnan(nextPos.x))
                            Gizmos.DrawLine(waypoint.pos, nextPos);
                    }
                }
            }

            // Draw connection points
            foreach (var pair in connectionPoints)
            {
                float3 pos = pair.Key;
                Connection connection = pair.Value;

                if (connection.output)
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = Color.green;

                Gizmos.DrawSphere(pos, 0.15f);
            }
        }

        private static unsafe void UpdateNext(float3 pos, ref NativeHashMap<float3, Waypoint> waypoints, ref NativeHashMap<float3, Connection> connections)
        {
            Waypoint waypoint = waypoints[pos];
            for (int i = 0; i < waypoint.next.Size; i++)
            {
                float3 nextPos = waypoint.next[i];
                if (!waypoints.ContainsKey(nextPos)) continue;
                waypoints[nextPos].RemovePrevious(pos);
                waypoints[nextPos].GetConnections( ref connections);
            }
        }
        private static unsafe void UpdatePrevious(float3 pos, ref NativeHashMap<float3, Waypoint> waypoints)
        {
            Waypoint waypoint = waypoints[pos];
            for (int i = 0; i < waypoint.next.Size; i++)
            {
                float3 prevPos = waypoint.previous[i];
                if (!waypoints.ContainsKey(prevPos)) continue;
                waypoints[prevPos].RemoveNext(pos);
            }
        }
        private static void DeleteWaypoint(float3 pos, ref NativeHashMap<float3, Waypoint> waypoints)
        {
            // Remove connections to waypoint
            UpdateNext(pos, ref waypoints);
            UpdatePrevious(pos, ref waypoints);

            // Remove waypoint and copy the last one to that position (references to last waypoint have already been updated)
            waypoints.Remove(pos);
        }

        public struct Connection
        {
            public float3 pos;
            public float3 waypoint;
            public bool output; // false => input
            public Connection(float3 pos, float3 waypoint, bool output)
            {
                this.pos = pos;
                this.waypoint = waypoint;
                this.output = output;
            }
        }
    }
}