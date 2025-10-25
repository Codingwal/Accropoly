using Components;
using Components.WaypointComponents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
[UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
public partial class InitNewWaypointsSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<WaypointsData>();
    }
    protected override void OnUpdate()
    {
        NativeHashMap<float3, Entity> waypoints = SystemAPI.GetSingleton<WaypointsData>().waypoints;

        // Add new waypoints to waypoints lookup
        foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<NewWaypoint>().WithEntityAccess())
        {
            waypoints.Add(transform.ValueRO.Position, entity);
        }

        new InitializeNewWaypoints()
        {
            ecb = SystemAPI.GetSingleton<BeginPresentationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged),
            waypoints = waypoints,
            tileGrid = SystemAPI.GetSingletonBuffer<EntityBufferElement>(),
            transportTileLookup = SystemAPI.GetComponentLookup<TransportTile>(),
            connectionsLookup = SystemAPI.GetComponentLookup<Connections>(),
        }.Schedule();
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
}
