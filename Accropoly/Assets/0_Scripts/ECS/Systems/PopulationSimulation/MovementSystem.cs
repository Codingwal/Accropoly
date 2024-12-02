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
        protected override void OnCreate()
        {
            RequireForUpdate<Traveller>();
            RequireForUpdate<RunGame>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            var buffer = SystemAPI.GetBuffer<EntityBufferElement>(SystemAPI.GetSingletonEntity<EntityGridHolder>());
            WorldTime time = SystemAPI.GetSingleton<GameInfo>().time;
            float deltaTime = SystemAPI.GetSingleton<GameInfo>().deltaTime;

            Entities.WithAll<Travelling>().ForEach((Entity entity, ref Traveller traveller) =>
            {
                LocalTransform transform = SystemAPI.GetComponent<LocalTransform>(entity);
                Waypoint waypoint = traveller.waypoints[traveller.nextWaypointIndex];

                // If this is the destination waypoint, jump directly on the destination tile
                if (traveller.nextWaypointIndex + 1 == traveller.waypoints.Length)
                {
                    transform.Position.xz = waypoint.pos * 2;
                    ecb.SetComponent(entity, transform);
                    ecb.SetComponentEnabled<Travelling>(entity, false);
                    Debug.Log(time);
                    return;
                }

                // Calculate the side on which the tile will be entered and exited using the previous and next waypoint
                Waypoint previousWaypoint = traveller.waypoints[traveller.nextWaypointIndex - 1];
                Direction entryDir = new(previousWaypoint.pos - waypoint.pos);
                Waypoint nextWaypoint = traveller.waypoints[traveller.nextWaypointIndex + 1];
                Direction exitDir = new(nextWaypoint.pos - waypoint.pos);

                // Make sure the tile contains all TransportTileAspect component
                Debug.Assert(SystemAPI.HasComponent<TransportTile>(TileGridUtility.GetTile(waypoint.pos, buffer)), "Expected a transport tile");

                // Calculate the position using the TransportTileAspect of the current tile
                var transportTileAspect = SystemAPI.GetAspect<TransportTileAspect>(TileGridUtility.GetTile(waypoint.pos, buffer));
                float2 pos = transportTileAspect.TravelOnTile(entryDir, exitDir, traveller.timeOnTile, out bool reachedTileEnd);

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
            }).WithBurst(Unity.Burst.FloatMode.Fast, Unity.Burst.FloatPrecision.Low).Schedule();
        }
    }
}