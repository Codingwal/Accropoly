using Components;
using Components.WaypointComponents;
using Tags;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
        var tileGrid = TileGridUtility.GetEntityGrid();
        var ecb = SystemAPI.GetSingleton<BeginPresentationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        // Add new waypoints to waypoints lookup
        foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<NewWaypoint>().WithEntityAccess())
        {
            waypoints.Add(transform.ValueRO.Position, entity);
        }

        // Add new waypoints to their transport tile
        foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<NewWaypoint>().WithEntityAccess())
        {
            // Add to waypoint list of the tile this waypoint belongs to
            int2 tilePos = (int2)math.round(transform.ValueRO.Position.xz / 2);
            Entity tile = TileGridUtility.GetTile(tilePos, tileGrid);
            SystemAPI.GetComponentRW<TransportTile>(tile).ValueRW.waypoints.Add(entity);

            ecb.RemoveComponent<NewWaypoint>(entity);
        }
    }
}
