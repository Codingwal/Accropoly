using Components;
using Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class WaypointSystem : SystemBase
    {
        private NativeHashMap<float3, Waypoint> waypointsTmp;
        private EntityQuery tilesToUpdate;
        private EntityQuery tileWithReplaceTag;
        protected override void OnCreate()
        {
            waypointsTmp = new(10, Allocator.TempJob);

            tilesToUpdate = new EntityQueryBuilder(Allocator.Temp)
                .WithAspect<TransportTileAspect>()
                .WithNone<Replace>()
                .Build(this);

            tileWithReplaceTag = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Replace>()
                .WithAspect<TransportTileAspect>()
                .Build(this);
        }
        protected override void OnUpdate()
        {
            if (SystemAPI.HasSingleton<LoadGame>())
            {
                // Create and initialize data
                WaypointsData data = new() { waypoints = new(30, Allocator.Persistent) };
                EntityManager.CreateSingleton(data);
            }
            else if (SystemAPI.HasSingleton<SaveGame>())
            {
                // Dispose data
                Entity dataHolder = SystemAPI.GetSingletonEntity<WaypointsData>();
                RefRW<WaypointsData> data = SystemAPI.GetComponentRW<WaypointsData>(dataHolder);
                data.ValueRW.waypoints.Dispose();
                EntityManager.DestroyEntity(dataHolder);
            }

            if (!(SystemAPI.HasSingleton<RunGame>() || SystemAPI.HasSingleton<LoadGame>()))
                return;

            RefRW<WaypointsData> waypointsData = SystemAPI.GetSingletonRW<WaypointsData>();

            new ClearReplaceTilesJob()
            {
                data = waypointsData
            }.Schedule(tileWithReplaceTag);

            new UpdateTilesJob()
            {
                data = waypointsData,
                waypointsTmp = waypointsTmp,
            }.Schedule(tilesToUpdate);
        }
        public void DrawGizmos()
        {
            if (!ECSUtility.TryGetSingleton(out WaypointsData data))
                return;

            // Draw waypoints and connections in between
            foreach (var pair in data.waypoints)
            {
                Waypoint waypoint = pair.Value;

                // Draw waypoint
                Gizmos.color = Color.blue;
                if (waypoint.exit)
                    Gizmos.color = Color.cyan;
                if (waypoint.registeredObjects > 0)
                    Gizmos.color = Color.green;
                if (waypoint.stop)
                    Gizmos.color = Color.red;
                Gizmos.DrawSphere(waypoint.pos, 0.1f);

                // Draw connections
                Gizmos.color = Color.blue;
                for (int i = 0; i < waypoint.next.Size; i++)
                {
                    float3 nextPos = waypoint.next[i];
                    if (!math.isnan(nextPos.x))
                        Gizmos.DrawLine(waypoint.pos, nextPos);
                }
            }
        }

        /// <summary>
        /// Remove waypoints from tiles that will get replaced
        /// </summary>
        [BurstCompile]
        private partial struct ClearReplaceTilesJob : IJobEntity
        {
            [NativeDisableUnsafePtrRestriction]
            public RefRW<WaypointsData> data;
            public void Execute(TransportTileAspect transportTileAspect)
            {
                DeleteTileWaypoints(ref transportTileAspect.transportTile.ValueRW.waypoints, data);
            }
        }

        /// <summary>
        /// Update tiles that should have waypoints (e.g. streets) (delete old waypoints if present, create new waypoints)
        /// </summary>
        [BurstCompile]
        [WithChangeFilter(typeof(ConnectingTile), typeof(Tile))] // For performance reasons: Only execute when a relevant component changed
        private partial struct UpdateTilesJob : IJobEntity
        {
            [NativeDisableUnsafePtrRestriction]
            public RefRW<WaypointsData> data;
            public NativeHashMap<float3, Waypoint> waypointsTmp;
            public void Execute(TransportTileAspect transportTileAspect)
            {
                // Delete all waypoints owned by this tile
                DeleteTileWaypoints(ref transportTileAspect.transportTile.ValueRW.waypoints, data);

                transportTileAspect.GetPoints(ref waypointsTmp);

                foreach (var pair in waypointsTmp)
                {
                    data.ValueRW.waypoints.Add(pair.Key, pair.Value);
                    float3 pos = pair.Key;

                    foreach (var otherPair in data.ValueRO.waypoints)
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
                            LinkWaypoints(pos, otherPos, data);
                        else // other -> this
                            LinkWaypoints(otherPos, pos, data);

                        break;
                    }
                }

                waypointsTmp.Clear();
            }
        }

        private static void DeleteTileWaypoints(ref FixedFloat3Array20 tileWaypoints, RefRW<WaypointsData> data)
        {
            for (int i = 0; i < tileWaypoints.Size; i++)
            {
                float3 pos = tileWaypoints[i];
                if (math.isnan(pos.x)) continue;
                Waypoint waypoint = data.ValueRO.waypoints[pos];

                // Update next
                for (int j = 0; j < waypoint.next.Size; j++)
                {
                    float3 other = waypoint.next[j];
                    if (math.isnan(other.x)) continue;

                    // All waypoints of this tile will get deleted => updating them is unneccessary
                    if (tileWaypoints.Contains(other))
                        continue;

                    Waypoint tmp = data.ValueRO.waypoints[other];
                    tmp.RemovePrevious(pos);
                    data.ValueRW.waypoints[other] = tmp;
                }

                // Update previous
                for (int j = 0; j < waypoint.previous.Size; j++)
                {
                    float3 other = waypoint.previous[j];
                    if (math.isnan(other.x)) continue;

                    // All waypoints of this tile will get deleted => updating them is unneccessary
                    if (tileWaypoints.Contains(other))
                        continue;

                    Waypoint tmp = data.ValueRO.waypoints[other];
                    tmp.RemoveNext(pos);
                    data.ValueRW.waypoints[other] = tmp;
                }

                data.ValueRW.waypoints.Remove(pos);
            }
            tileWaypoints.Clear(float.NaN);
        }
        private static void LinkWaypoints(float3 from, float3 to, RefRW<WaypointsData> data)
        {
            Waypoint copy = data.ValueRO.waypoints[from];
            copy.AddNext(to);
            data.ValueRW.waypoints[from] = copy;

            copy = data.ValueRO.waypoints[to];
            copy.AddPrevious(from);
            data.ValueRW.waypoints[to] = copy;
        }
    }
}