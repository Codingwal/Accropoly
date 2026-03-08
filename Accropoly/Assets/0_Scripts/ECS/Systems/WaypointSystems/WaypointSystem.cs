using System;
using Components;
using Components.WaypointComponents;
using Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    /// <summary>
    /// Manages all waypoints (creation, deletion, updating)
    /// </summary>
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class WaypointSystem : SystemBase
    {
        private bool firstUpdate = true;

        protected override void OnUpdate()
        {
            if (firstUpdate)
            {
                WaypointsData data = new() { waypoints = new(30, Allocator.Persistent) };
                EntityManager.CreateSingleton(data);
                firstUpdate = false;
            }

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            WaypointsData waypointsData = SystemAPI.GetComponent<WaypointsData>(SystemAPI.GetSingletonEntity<WaypointsData>());

            // Needed by jobs
            JobUtility jobUtility = new()
            {
                transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(),
                ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged), // Needs a seperate ecb
                waypoints = waypointsData.waypoints,
            };

            new ClearReplaceTilesJob()
            {
                jobUtility = jobUtility
            }.Schedule();

            // Garantee that all connecting tiles have been updated
            World.GetExistingSystemManaged<TileConnectionSystem>().CheckedStateRef.Dependency.Complete();

            new UpdateTilesJob()
            {
                ecb = ecb,
                jobUtility = jobUtility,
                connectingTileLookup = SystemAPI.GetComponentLookup<ConnectingTile>(isReadOnly: true),
                connectingTileOldLookup = SystemAPI.GetComponentLookup<CopyComponent<ConnectingTile>>()
            }.Schedule();

            // Ugly and slow but neccessary :(
            Dependency.Complete();
            SystemAPI.SetComponent(SystemAPI.GetSingletonEntity<WaypointsData>(), waypointsData);
        }

        protected override void OnDestroy()
        {
            SystemAPI.GetSingleton<WaypointsData>().waypoints.Dispose();
        }

        public void DrawGizmos(bool displayJunctionInfo, bool hideUnusedConnections)
        {
            if (!SystemAPI.HasSingleton<WaypointsData>())
                return;

            WaypointsData waypointsData = SystemAPI.GetSingleton<WaypointsData>();

            foreach (var (transform, connections, waypoint, entity) in SystemAPI.Query<RefRO<LocalTransform>, DynamicBuffer<Connection>, RefRO<Waypoint>>().WithEntityAccess())
            {
                // Select color depending on waypoint type
                Gizmos.color = waypoint.ValueRO.allowedObjects switch
                {
                    TravelObjects.Street => Color.blue,
                    TravelObjects.Sidewalk => Color.cyan,
                    _ => Color.magenta
                };

                // Display additional data for junctions (by changing the colour)
                if (displayJunctionInfo && SystemAPI.HasComponent<Junction>(entity))
                {
                    var junction = SystemAPI.GetComponentRO<Junction>(entity);
                    if (junction.ValueRO.registeredObjects > 0)

                        Gizmos.color = Color.green;
                    if (junction.ValueRO.stop)
                        Gizmos.color = Color.red;
                }

                Gizmos.DrawSphere(transform.ValueRO.Position, 0.1f);

                // Draw connections
                Gizmos.color = Color.blue;
                foreach (Connection connection in connections)
                {
                    if (hideUnusedConnections && !waypointsData.waypoints.ContainsKey(connection.nextWaypoint))
                        continue;

                    new BezierCurve(transform.ValueRO.Position, connection.controlPoint, connection.nextWaypoint).Draw(10);
                }
            }
        }

        /// <summary>
        /// Remove waypoints from tiles that will get replaced
        /// </summary>
        [BurstCompile]
        [WithAll(typeof(Replace))]
        private partial struct ClearReplaceTilesJob : IJobEntity
        {
            public JobUtility jobUtility;
            public void Execute(ref TransportTile transportTile)
            {
                jobUtility.DeleteTileWaypoints(ref transportTile.waypoints);
                transportTile.waypoints.Dispose();
            }
        }

        /// <summary>
        /// Update tiles that should have waypoints (e.g. streets) (delete old waypoints if present, create new waypoints)
        /// </summary>
        [BurstCompile]
        private partial struct UpdateTilesJob : IJobEntity
        {
            public EntityCommandBuffer ecb;
            public JobUtility jobUtility;
            [ReadOnly] public ComponentLookup<ConnectingTile> connectingTileLookup;
            public ComponentLookup<CopyComponent<ConnectingTile>> connectingTileOldLookup;
            public void Execute(Entity entity, ref TransportTile transportTile, in Tile tile, ref CopyComponent<Tile> tileOld, in LocalTransform transform)
            {
                if (DataChanged(entity, in tile, in tileOld))
                {
                    // Delete all waypoints owned by this tile
                    jobUtility.DeleteTileWaypoints(ref transportTile.waypoints);

                    // Check if the tile is a connecting tile
                    ConnectingTile? connectingTile = null;
                    if (connectingTileLookup.HasComponent(entity))
                        connectingTile = connectingTileLookup[entity];

                    // Create new waypoints
                    var tileWaypointUtility = new TileWaypointUtility(tile, transform, connectingTile);
                    tileWaypointUtility.CreateWaypoints(ref ecb);
                }

                tileOld.value = tile;
                if (connectingTileLookup.HasComponent(entity))
                    connectingTileOldLookup[entity] = new(connectingTileLookup[entity]);
            }

            private bool DataChanged(Entity entity, in Tile tile, in CopyComponent<Tile> tileOld)
            {
                // Tiletype or rotation changed?
                if (tile.tileType != tileOld.value.tileType || tile.rotation != tileOld.value.rotation)
                    return true;

                // If not a connecting tile, nothing relevant changed
                if (!connectingTileLookup.HasComponent(entity))
                    return false;

                // Not equal => something changed
                return connectingTileLookup[entity].index != connectingTileOldLookup[entity].value.index;
            }
        }

        private struct JobUtility
        {
            [NativeDisableContainerSafetyRestriction] public ComponentLookup<LocalTransform> transformLookup;
            public EntityCommandBuffer ecb;
            public NativeHashMap<float3, Entity> waypoints;
            public void DeleteTileWaypoints(ref UnsafeList<Entity> tileWaypoints)
            {
                foreach (Entity entity in tileWaypoints)
                {
                    float3 pos = transformLookup.GetRefRO(entity).ValueRO.Position;

                    ecb.DestroyEntity(entity);
                    waypoints.Remove(pos);
                }
                tileWaypoints.Clear();
            }
        }
    }
}