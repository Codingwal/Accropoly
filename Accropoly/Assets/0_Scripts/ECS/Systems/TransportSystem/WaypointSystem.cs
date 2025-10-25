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
        private EntityQuery connectingTilesToUpdate;
        private EntityQuery otherTilesToUpdate;
        private EntityQuery tileWithReplaceTag;
        protected override void OnCreate()
        {
            // Contains all TransportTiles where relevant data changed (including all NewTiles) (includes all TransportTiles at world loading)
            connectingTilesToUpdate = new EntityQueryBuilder(Allocator.Temp)
                .WithAspect<TransportTileAspect>()
                .WithAll<ConnectingTile>() // Needed for SetChangedVersionFilter
                .WithNone<Replace>()
                .Build(this);
            connectingTilesToUpdate.SetChangedVersionFilter(new ComponentType[] { typeof(ConnectingTile), typeof(Tile) });

            otherTilesToUpdate = new EntityQueryBuilder(Allocator.Temp)
                .WithAspect<TransportTileAspect>()
                .WithNone<Replace, ConnectingTile>()
                .Build(this);
            otherTilesToUpdate.SetChangedVersionFilter(new ComponentType[] { typeof(Tile) });

            tileWithReplaceTag = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Replace, TransportTile>()
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
                // Dispose and destroy WaypointsData
                Entity dataHolder = SystemAPI.GetSingletonEntity<WaypointsData>();
                RefRW<WaypointsData> data = SystemAPI.GetComponentRW<WaypointsData>(dataHolder);
                data.ValueRW.waypoints.Dispose();
                EntityManager.DestroyEntity(dataHolder);

                // Destroy waypoints
                EntityManager.DestroyEntity(GetEntityQuery(typeof(Waypoint)));

                // Dispose TransportTile data
                foreach (var transportTile in SystemAPI.Query<RefRW<TransportTile>>())
                {
                    transportTile.ValueRW.waypoints.Dispose();
                }
            }

            if (!(SystemAPI.HasSingleton<RunGame>() || SystemAPI.HasSingleton<LoadGame>()))
                return;

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            WaypointsData waypointsData = SystemAPI.GetComponent<WaypointsData>(SystemAPI.GetSingletonEntity<WaypointsData>());

            // Add new waypoints to waypoints lookup
            foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<NewWaypoint>().WithEntityAccess())
            {
                waypointsData.waypoints.Add(transform.ValueRO.Position, entity);
            }

            // Needed by jobs
            JobUtility jobUtility = new()
            {
                connectionsLookup = SystemAPI.GetComponentLookup<Connections>(),
                transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(),
                ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged), // Needs a seperate ecb
                waypoints = waypointsData.waypoints,
            };

            new InitializeNewWaypoints()
            {
                ecb = ecb,
                waypoints = waypointsData.waypoints,
                tileGrid = SystemAPI.GetSingletonBuffer<EntityBufferElement>(),
                transportTileLookup = SystemAPI.GetComponentLookup<TransportTile>(),
                connectionsLookup = SystemAPI.GetComponentLookup<Connections>(),
            }.Schedule();

            new ClearReplaceTilesJob()
            {
                jobUtility = jobUtility
            }.Schedule(tileWithReplaceTag);

            // Garantee that all connecting tiles have been updated
            World.GetExistingSystemManaged<TileConnectionSystem>().CheckedStateRef.Dependency.Complete();

            new UpdateTilesJob()
            {
                ecb = ecb,
                jobUtility = jobUtility,
            }.Schedule(connectingTilesToUpdate);
            new UpdateTilesJob()
            {
                ecb = ecb,
                jobUtility = jobUtility,
            }.Schedule(otherTilesToUpdate);

            // Ugly and slow but neccessary :(
            Dependency.Complete();
            SystemAPI.SetComponent(SystemAPI.GetSingletonEntity<WaypointsData>(), waypointsData);
        }
        public void DrawGizmos(bool highlightTileExits, bool displayJunctionInfo)
        {
            foreach (var (transform, connections, waypoint, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<Connections>, RefRO<Waypoint>>().WithEntityAccess())
            {
                // Select color depending on waypoint type
                Gizmos.color = waypoint.ValueRO.allowedObjects switch
                {
                    TravelObjects.Street => Color.blue,
                    TravelObjects.Sidewalk => Color.cyan,
                    _ => Color.magenta
                };

                if (highlightTileExits && connections.ValueRO.exit)
                    Gizmos.color = Color.gray;

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
                foreach (Entity nextEntity in connections.ValueRO.next)
                {
                    if (nextEntity == Entity.Null)
                        continue;

                    float3 nextPos = SystemAPI.GetComponent<LocalTransform>(nextEntity).Position;
                    Gizmos.DrawLine(transform.ValueRO.Position, nextPos);
                }
            }
        }

        [BurstCompile]
        private partial struct InitializeNewWaypoints : IJobEntity
        {
            public EntityCommandBuffer ecb;
            public NativeHashMap<float3, Entity> waypoints;
            public DynamicBuffer<EntityBufferElement> tileGrid;
            public ComponentLookup<TransportTile> transportTileLookup;
            public ComponentLookup<Connections> connectionsLookup;
            public void Execute(Entity entity, in NewWaypoint newWaypoint, in LocalTransform transform)
            {
                var connections = connectionsLookup.GetRefRW(entity);

                // Add to waypoint list of the tile this waypoint belongs to
                int2 tilePos = (int2)math.round(transform.Position.xz / 2);
                Entity tile = TileGridUtility.GetTile(tilePos, tileGrid);
                transportTileLookup.GetRefRW(tile).ValueRW.waypoints.Add(entity);

                // Create connections
                foreach (float3 nextPos in newWaypoint.nextWaypoints)
                {
                    if (math.isnan(nextPos.x)) continue;
                    Debug.Assert(waypoints.ContainsKey(nextPos), $"Connection to non-existent waypoint found (from {transform.Position} to {nextPos})");
                    Entity next = waypoints[nextPos];
                    connections.ValueRW.AddNext(next);
                    connectionsLookup.GetRefRW(next).ValueRW.AddPrevious(entity);
                }

                // Connect with close waypoints
                foreach (var pair in waypoints)
                {
                    float3 otherPos = pair.Key;
                    Entity other = pair.Value;

                    // Skip self
                    if (otherPos.Equals(transform.Position))
                        continue;

                    // Skip if not close enough
                    if (math.lengthsq(transform.Position - otherPos) > math.square(0.15))
                        continue;

                    // Connect
                    var connectionsOther = connectionsLookup.GetRefRW(other);
                    if (connections.ValueRO.entry) // other -> this
                    {
                        Debug.Assert(connectionsOther.ValueRO.exit);
                        connectionsOther.ValueRW.AddNext(entity);
                        connections.ValueRW.AddPrevious(other);
                    }
                    if (connections.ValueRO.exit) // this -> other
                    {
                        Debug.Assert(connectionsOther.ValueRO.entry);
                        connections.ValueRW.AddNext(other);
                        connectionsOther.ValueRW.AddPrevious(entity);
                    }
                }

                ecb.RemoveComponent<NewWaypoint>(entity);
            }
        }

        /// <summary>
        /// Remove waypoints from tiles that will get replaced
        /// </summary>
        [BurstCompile]
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
            public void Execute(TransportTileAspect transportTileAspect)
            {
                // Delete all waypoints owned by this tile
                jobUtility.DeleteTileWaypoints(ref transportTileAspect.transportTile.ValueRW.waypoints);

                // Create new waypoints
                transportTileAspect.GetPoints(ecb);
            }
        }

        private struct JobUtility
        {
            public ComponentLookup<Connections> connectionsLookup;
            [NativeDisableContainerSafetyRestriction] public ComponentLookup<LocalTransform> transformLookup;
            public EntityCommandBuffer ecb;
            public NativeHashMap<float3, Entity> waypoints;
            public void DeleteTileWaypoints(ref UnsafeList<Entity> tileWaypoints)
            {
                foreach (Entity entity in tileWaypoints)
                {
                    float3 pos = transformLookup.GetRefRO(entity).ValueRO.Position;
                    RefRO<Connections> connections = connectionsLookup.GetRefRO(entity);

                    // Update next
                    foreach (Entity other in connections.ValueRO.next)
                    {
                        if (other == Entity.Null) continue;

                        // All waypoints of this tile will get deleted => updating them is unneccessary
                        if (tileWaypoints.Contains(other))
                            continue;

                        // Update other
                        RefRW<Connections> connectionsOther = connectionsLookup.GetRefRW(other);
                        connectionsOther.ValueRW.RemovePrevious(entity);
                    }

                    // Update previous
                    foreach (Entity other in connections.ValueRO.previous)
                    {
                        if (other == Entity.Null) continue;

                        // All waypoints of this tile will get deleted => updating them is unneccessary
                        if (tileWaypoints.Contains(other))
                            continue;

                        // Update other
                        RefRW<Connections> connectionsOther = connectionsLookup.GetRefRW(other);
                        connectionsOther.ValueRW.RemoveNext(entity);
                    }

                    ecb.DestroyEntity(entity);
                    waypoints.Remove(pos);
                }
                tileWaypoints.Clear();
            }
        }
    }
}