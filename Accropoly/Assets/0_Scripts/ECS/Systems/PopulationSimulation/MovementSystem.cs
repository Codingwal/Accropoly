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
                Road[] roads = SystemAPI.GetAspect<TransportTileAspect>(TileGridUtility.GetTile(waypoint.pos, buffer)).GetRoads();

                // Select a road
                Road road = new();
                foreach (Road r in roads)
                    if (r.exitDirection == dir)
                        road = r;

                float2 nextPoint = road.points[traveller.nextPointIndex];

                if (math.distancesq(transform.Position.xz, nextPoint * 2) <= waypointRangeSqr)
                {
                    traveller.nextPointIndex++;

                    if (traveller.nextPointIndex == road.points.Length) // If the final point has been reached, move on to next waypoint
                    {
                        traveller.nextWaypointIndex++;
                        traveller.nextPointIndex = 0;

                        if (traveller.nextWaypointIndex == traveller.waypoints.Length) // If the destination has been reached
                        {
                            ecb.SetComponentEnabled<Travelling>(entity, false);
                            return;
                        }

                        waypoint = traveller.waypoints[traveller.nextWaypointIndex];
                    }
                }

                float2 direction = nextPoint * 2 - transform.Position.xz;
                float2 directionNormalized = math.normalize(direction);
                transform.Position.xz += deltaTime * speed * directionNormalized;

                ecb.SetComponent(entity, transform);
            }).WithBurst(Unity.Burst.FloatMode.Fast, Unity.Burst.FloatPrecision.Low).Schedule();
        }
    }
}