using Components;
using Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    public partial class MovementSystem : SystemBase
    {
        private const float speed = 6f;
        private const float waypointRange = 0.1f;
        private const float waypointRangeSqr = waypointRange * waypointRange;
        protected override void OnCreate()
        {
            RequireForUpdate<Traveller>();
            RequireForUpdate<RunGame>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            var buffer = SystemAPI.GetBuffer<EntityBufferElement>(SystemAPI.GetSingletonEntity<EntityGridHolder>());
            float deltaTime = SystemAPI.Time.DeltaTime;

            Entities.WithAll<Travelling>().ForEach((Entity entity, ref Traveller traveller) =>
            {
                LocalTransform transform = SystemAPI.GetComponent<LocalTransform>(entity);
                Waypoint waypoint = traveller.waypoints[traveller.nextWaypointIndex];

                // If this is the destination waypoint, jump directly on the destination tile
                if (traveller.nextWaypointIndex + 1 == traveller.waypoints.Length)
                {
                    transform.Position.xz = waypoint.pos;
                    ecb.SetComponentEnabled<Travelling>(entity, false);
                    return;
                }

                // Calculate the side on which the current tile should be left
                Waypoint nextWaypoint = traveller.waypoints[traveller.nextWaypointIndex + 1];
                Direction dir = new(nextWaypoint.pos - waypoint.pos);

                // Make sure the tile contains all TransportTileAspect component
                Debug.Assert(SystemAPI.HasComponent<TransportTile>(TileGridUtility.GetTile(waypoint.pos, buffer)));

                // Calculate the position using the TransportTileAspect of the current tile
                var transportTileAspect = SystemAPI.GetAspect<TransportTileAspect>(TileGridUtility.GetTile(waypoint.pos, buffer));
                float2 pos = transportTileAspect.TravelOnTile(dir, traveller.timeOnTile, out bool reachedTileEnd);

                // Update the time spent on the current tile
                traveller.timeOnTile += deltaTime;

                // Move on to the next tile if the current tile has been completely traversed
                if (reachedTileEnd)
                {
                    traveller.nextWaypointIndex++;
                    traveller.timeOnTile = 0;
                    waypoint = traveller.waypoints[traveller.nextWaypointIndex];
                }

                // Update and store the transform
                transform.Position.xz = pos;
                ecb.SetComponent(entity, transform);

                Debug.Log($"Current tile: {waypoint.pos}");
            }).WithBurst(Unity.Burst.FloatMode.Fast, Unity.Burst.FloatPrecision.Low).Schedule();
        }
    }
}