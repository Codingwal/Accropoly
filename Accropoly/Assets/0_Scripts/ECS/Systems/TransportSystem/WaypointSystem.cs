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
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class WaypointSystem : SystemBase
    {
        private EntityQuery tilesToUpdate;
        private EntityQuery tileWithReplaceTag;
        protected override void OnCreate()
        {
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

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            RefRW<WaypointsData> waypointsData = SystemAPI.GetSingletonRW<WaypointsData>();

            // Add new waypoints to waypoints lookup
            foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<NewWaypoint>().WithEntityAccess())
            {
                waypointsData.ValueRW.waypoints.Add(transform.ValueRO.Position, entity);
            }

            // Needed by jobs
            JobUtility jobUtility = new JobUtility()
            {
                connectionsLookup = SystemAPI.GetComponentLookup<Connections>(),
                transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(),
                ecb = ecb,
                data = waypointsData
            };

            new InitializeNewWaypoints()
            {
                ecb = ecb,
                data = waypointsData,
                tileGrid = TileGridUtility.GetEntityGrid(),
                transportTileLookup = SystemAPI.GetComponentLookup<TransportTile>(),
                connectionsLookup = SystemAPI.GetComponentLookup<Connections>(),
            }.Schedule();

            new ClearReplaceTilesJob()
            {
                jobUtility = jobUtility
            }.Schedule(tileWithReplaceTag);

            new UpdateTilesJob()
            {
                ecb = ecb,
                jobUtility = jobUtility,
            }.Schedule(tilesToUpdate);
        }
        public void DrawGizmos()
        {
            foreach (var (transform, junction, connections) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<Junction>, RefRO<Connections>>().WithAll<Waypoint>())
            {
                // Draw waypoint
                Gizmos.color = Color.blue;
                if (junction.ValueRO.registeredObjects > 0)
                    Gizmos.color = Color.green;
                if (junction.ValueRO.stop)
                    Gizmos.color = Color.red;
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
            public RefRW<WaypointsData> data;
            public DynamicBuffer<EntityBufferElement> tileGrid;
            public ComponentLookup<TransportTile> transportTileLookup;
            public ComponentLookup<Connections> connectionsLookup;
            public void Execute(Entity entity, ref Connections connections, in NewWaypoint newWaypoint, in LocalTransform transform)
            {
                // Add to waypoint list of the tile this waypoint belongs to
                int2 tilePos = (int2)math.round(transform.Position.xz / 2) * 2;
                Entity tile = TileGridUtility.GetTile(tilePos, tileGrid);
                transportTileLookup.GetRefRW(tile).ValueRW.AddWaypoint(entity);

                // Create connections
                foreach (float3 nextPos in newWaypoint.nextWaypoints)
                {
                    Entity next = data.ValueRW.waypoints[nextPos];
                    connections.AddNext(nextPos);
                    connectionsLookup.GetRefRW(next).ValueRW.AddPrevious(transform.Position);
                }

                // Connect with close waypoints
                foreach (var pair in data.ValueRO.waypoints)
                {
                    float3 otherPos = pair.Key;
                    Entity other = pair.Value;

                    // Skip self
                    if (otherPos.Equals(transform.Position))
                        continue;

                    // Skip if not close enough
                    if (math.lengthsq(transform.Position - otherPos) > 0.05)
                        continue;

                    // Connect
                    var connectionsOther = connectionsLookup.GetRefRW(other);
                    if (connections.exit && !connectionsOther.ValueRO.exit) // this -> other
                    {
                        connections.AddNext(otherPos);
                        connectionsOther.ValueRW.AddPrevious(transform.Position);
                    }
                    else if (!connections.exit && connectionsOther.ValueRO.exit) // other -> this
                    {
                        connectionsOther.ValueRW.AddNext(transform.Position);
                        connections.AddPrevious(otherPos);
                    }
                    else // Both are exits / entries
                        throw new();
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
            public void Execute(TransportTileAspect transportTileAspect)
            {
                jobUtility.DeleteTileWaypoints(ref transportTileAspect.transportTile.ValueRW.waypoints);
            }
        }

        /// <summary>
        /// Update tiles that should have waypoints (e.g. streets) (delete old waypoints if present, create new waypoints)
        /// </summary>
        [BurstCompile]
        [WithChangeFilter(typeof(ConnectingTile), typeof(Tile))] // For performance reasons: Only execute when a relevant component changed
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
            public ComponentLookup<LocalTransform> transformLookup;
            public EntityCommandBuffer ecb;
            [NativeDisableUnsafePtrRestriction] public RefRW<WaypointsData> data;
            public void DeleteTileWaypoints(ref FixedEntityArray20 tileWaypoints)
            {
                foreach (Entity entity in tileWaypoints)
                {
                    if (entity == Entity.Null) continue;

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
                        connectionsOther.ValueRW.RemovePrevious(pos);
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
                        connectionsOther.ValueRW.RemoveNext(pos);
                    }

                    ecb.DestroyEntity(entity);
                    data.ValueRW.waypoints.Remove(pos);
                }
                tileWaypoints.Clear(Entity.Null);
            }
        }
    }
}