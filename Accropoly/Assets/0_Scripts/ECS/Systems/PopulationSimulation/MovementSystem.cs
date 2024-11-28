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
                Waypoint nextWaypoint = traveller.waypoints[traveller.nextWaypointIndex + 1];
                Direction dir = new(nextWaypoint.pos - waypoint.pos);
                var transportTileAspect = SystemAPI.GetAspect<TransportTileAspect>(TileGridUtility.GetTile(waypoint.pos, buffer));

                float2 pos = transportTileAspect.TravelOnTile(dir, traveller.timeOnTile, out bool reachedTileEnd);

                if (reachedTileEnd)
                {
                    traveller.nextWaypointIndex++;
                    traveller.timeOnTile = 0;

                    if (traveller.nextWaypointIndex == traveller.waypoints.Length) // If the destination has been reached
                    {
                        Debug.LogError("!");
                        ecb.SetComponentEnabled<Travelling>(entity, false);
                        return;
                    }

                    waypoint = traveller.waypoints[traveller.nextWaypointIndex];
                }

                traveller.timeOnTile += deltaTime;

                transform.Position.xz = pos;
                ecb.SetComponent(entity, transform);

                Debug.Log($"Current tile: {waypoint.pos}");
            }).WithBurst(Unity.Burst.FloatMode.Fast, Unity.Burst.FloatPrecision.Low).Schedule();
        }
    }
}