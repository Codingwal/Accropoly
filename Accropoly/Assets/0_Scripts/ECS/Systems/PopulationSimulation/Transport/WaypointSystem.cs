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
        public static NativeList<Waypoint> waypoints;
        public static NativeHashMap<float3, Connection> connectionPoints;
        protected override void OnCreate()
        {
            waypoints = new(Allocator.Persistent);
            connectionPoints = new(30, Allocator.Persistent);
        }
        protected override void OnUpdate()
        {
            NativeList<Connection> connectionsTmp = new(4, Allocator.TempJob);
            Entities.WithChangeFilter<ConnectingTile>().ForEach((TransportTileAspect transportTileAspect, in Tile tile) =>
            {
                // Delete all waypoints owned by this tile
                unsafe
                {
                    for (int i = 0; i < TransportTile.maxWaypoints; i++)
                    {
                        int waypointIndex = transportTileAspect.TransportTile.GetWaypoint(i);
                        if (waypointIndex == -1) continue;
                        DeleteWaypoint(waypointIndex, ref waypoints);
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
                            // Use ElementAt as the element needs to be modified
                            waypoints.ElementAt(connection.waypointIndex).AddNext(other.waypointIndex);
                            waypoints.ElementAt(other.waypointIndex).AddPrevious(connection.waypointIndex);
                        }
                        else // other -> this
                        {
                            // Use ElementAt as the element needs to be modified
                            waypoints.ElementAt(other.waypointIndex).AddNext(connection.waypointIndex);
                            waypoints.ElementAt(connection.waypointIndex).AddPrevious(other.waypointIndex);
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
            foreach (var waypoint in waypoints)
            {
                Gizmos.DrawSphere(waypoint.pos, 0.15f);

                unsafe
                {
                    for (int i = 0; i < Waypoint.maxConnections; i++)
                    {
                        int nextIndex = waypoint.next[i];
                        if (nextIndex != -1)
                            Gizmos.DrawLine(waypoint.pos, waypoints[nextIndex].pos);
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

        private static unsafe void UpdateNext(int waypointIndex, int newIndex, ref NativeList<Waypoint> waypoints)
        {
            Waypoint waypoint = waypoints[waypointIndex];
            for (int i = 0; i < Waypoint.maxConnections; i++)
            {
                int nextIndex = waypoint.next[i];
                if (nextIndex == -1) continue;
                waypoints[nextIndex].UpdatePrevious(waypointIndex, newIndex);
            }
        }
        private static unsafe void UpdatePrevious(int waypointIndex, int newIndex, ref NativeList<Waypoint> waypoints)
        {
            Waypoint waypoint = waypoints[waypointIndex];
            for (int i = 0; i < Waypoint.maxConnections; i++)
            {
                int prevIndex = waypoint.previous[i];
                if (prevIndex == -1) continue;
                waypoints[prevIndex].UpdateNext(waypointIndex, newIndex);
            }
        }
        private static void DeleteWaypoint(int waypointIndex, ref NativeList<Waypoint> waypoints)
        {
            // Remove connections to waypoint
            UpdateNext(waypointIndex, -1, ref waypoints);
            UpdatePrevious(waypointIndex, -1, ref waypoints);

            // Move references of the last waypoint to the index of the one getting deleted (preperation for next step)
            int lastWaypointIndex = waypoints.Length - 1;
            UpdateNext(lastWaypointIndex, waypointIndex, ref waypoints);
            UpdatePrevious(lastWaypointIndex, waypointIndex, ref waypoints);

            // Remove waypoint and copy the last one to that position (references to last waypoint have already been updated)
            waypoints.RemoveAtSwapBack(waypointIndex);
        }

        public struct Connection
        {
            public float3 pos;
            public int waypointIndex;
            public bool output; // false => input
            public Connection(float3 pos, int waypointIndex, bool output)
            {
                this.pos = pos;
                this.waypointIndex = waypointIndex;
                this.output = output;
            }
        }
    }
}