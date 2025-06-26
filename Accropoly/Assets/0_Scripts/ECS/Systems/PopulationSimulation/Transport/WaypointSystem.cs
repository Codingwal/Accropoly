using Components;
using Tags;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class WaypointSystem : SystemBase
    {
        private static NativeHashMap<float3, Waypoint> waypoints;
        private static NativeHashMap<float3, Waypoint> waypointsTmp; // Only declared here for efficiency
        protected override void OnCreate()
        {
            waypoints = new(30, Allocator.Persistent);
            waypointsTmp = new(10, Allocator.Persistent);
        }
        protected override void OnUpdate()
        {
            Entities.WithAll<Replace>().ForEach((TransportTileAspect transportTileAspect) =>
            {
                DeleteTileWaypoints(ref transportTileAspect.transportTile.ValueRW.waypoints);
            }).Schedule();

            Entities.WithNone<Replace>().WithChangeFilter<ConnectingTile>().ForEach((TransportTileAspect transportTileAspect) =>
            {
                DeleteTileWaypoints(ref transportTileAspect.transportTile.ValueRW.waypoints);

                transportTileAspect.GetPoints(ref waypointsTmp);

                foreach (var pair in waypointsTmp)
                {
                    waypoints.Add(pair.Key, pair.Value);
                    float3 pos = pair.Key;

                    foreach (var otherPair in waypoints)
                    {
                        float3 otherPos = otherPair.Key;

                        // Skip yourself
                        if (otherPos.Equals(pos))
                            continue;

                        if (math.lengthsq(pos - otherPos) > 0.05)
                            continue;

                        // Waypoints are extremely close together and should get connected
                        Debug.Assert(pair.Value.exit != otherPair.Value.exit, "Can't connect two exits/entries");
                        if (pair.Value.exit) // this -> other
                            LinkWaypoints(pos, otherPos);
                        else // other -> this
                            LinkWaypoints(otherPos, pos);

                        break;
                    }
                }

                waypointsTmp.Clear();
            }).WithoutBurst().Schedule();
        }
        protected override void OnDestroy()
        {
            waypoints.Dispose();
            waypointsTmp.Dispose();
        }

        public void DrawGizmos()
        {
            // Draw waypoints and connections in between
            foreach (var pair in waypoints)
            {
                Waypoint waypoint = pair.Value;

                if (waypoint.exit)
                    Gizmos.color = Color.cyan;
                else
                    Gizmos.color = Color.blue;

                Gizmos.DrawSphere(waypoint.pos, 0.15f);

                Gizmos.color = Color.blue;
                for (int i = 0; i < waypoint.next.Size; i++)
                {
                    float3 nextPos = waypoint.next[i];
                    if (!math.isnan(nextPos.x))
                        Gizmos.DrawLine(waypoint.pos, nextPos);
                }
            }
        }

        // Delete all waypoints owned by this tile
        private static void DeleteTileWaypoints(ref FixedFloat3Array20 tileWaypoints)
        {
            for (int i = 0; i < tileWaypoints.Size; i++)
            {
                float3 pos = tileWaypoints[i];
                if (math.isnan(pos.x)) continue;
                Waypoint waypoint = waypoints[pos];

                // Update next
                for (int j = 0; j < waypoint.next.Size; j++)
                {
                    float3 other = waypoint.next[j];
                    if (math.isnan(other.x)) continue;

                    // All waypoints of this tile will get deleted => updating them is unneccessary
                    if (tileWaypoints.Contains(other))
                        continue;

                    Waypoint tmp = waypoints[other];
                    tmp.RemovePrevious(pos);
                    waypoints[other] = tmp;
                }

                // Update previous
                for (int j = 0; j < waypoint.previous.Size; j++)
                {
                    float3 other = waypoint.previous[j];
                    if (math.isnan(other.x)) continue;

                    // All waypoints of this tile will get deleted => updating them is unneccessary
                    if (tileWaypoints.Contains(other))
                        continue;

                    Waypoint tmp = waypoints[other];
                    tmp.RemoveNext(pos);
                    waypoints[other] = tmp;
                }

                waypoints.Remove(pos);
            }
            tileWaypoints.Clear(float.NaN);
        }
        private static void LinkWaypoints(float3 from, float3 to)
        {
            Waypoint copy = waypoints[from];
            copy.AddNext(to);
            waypoints[from] = copy;

            copy = waypoints[to];
            copy.AddPrevious(from);
            waypoints[to] = copy;
        }
    }
}