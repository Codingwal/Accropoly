using Components;
using Tags;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    public partial class PathfindingSystem : SystemBase
    {
        private static ComponentLookup<TransportTile> transportTilesLookup;
        private static Unity.Mathematics.Random rnd;
        protected override void OnCreate()
        {
            RequireForUpdate<Traveller>();
            RequireForUpdate<RunGame>();

            transportTilesLookup = GetComponentLookup<TransportTile>(isReadOnly: true);
            rnd = new(1);
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            var buffer = SystemAPI.GetBuffer<EntityBufferElement>(SystemAPI.GetSingletonEntity<EntityGridHolder>());
            transportTilesLookup.Update(this);

            Entities.WithAll<WantsToTravel>().ForEach((Entity entity, ref Traveller traveller, in LocalTransform transform) =>
            {
                traveller.nextWaypointIndex = 0;

                if (traveller.waypoints.IsCreated)
                    traveller.waypoints.Clear();
                else
                    traveller.waypoints = new(8, Unity.Collections.Allocator.Persistent, Unity.Collections.NativeArrayOptions.UninitializedMemory);

                FindPath(ref traveller.waypoints, (int2)math.round(transform.Position.xz) / 2, traveller.destination, buffer);

                ecb.SetComponentEnabled<Travelling>(entity, true);
                ecb.SetComponentEnabled<WantsToTravel>(entity, false);
            }).Schedule();
        }
        private static void FindPath(ref UnsafeList<Waypoint> waypoints, int2 start, int2 dest, in DynamicBuffer<EntityBufferElement> buffer)
        {
            // Both start and dest must tile center
            Debug.Assert(!start.Equals(dest), "The start must not equal the destination");

            const int maxIterations = 1000;
            int i = 0;

            int2 pos = start;
            while (!pos.Equals(dest))
            {
                int2 dir = new();
                foreach (Direction direction in Direction.GetDirections())
                {
                    int2 newPos = pos + direction.DirectionVec;

                    if (newPos.Equals(dest))
                    {
                        waypoints.Add(new Waypoint { pos = newPos });
                        return;
                    }
                    if (transportTilesLookup.HasComponent(TileGridUtility.GetTile(newPos, buffer)) && !waypoints.Contains(new Waypoint { pos = newPos }))
                    {
                        dir = direction.DirectionVec;
                        break;
                    }
                }
                pos += dir;
                waypoints.Add(new Waypoint { pos = pos });

                if (i > maxIterations) throw new($"Reached the maximum amount of iterations while searching a path from {start} to {dest}! Stuck at {pos}");
                i++;
            }

            // while (pos.x != dest.x)
            // {
            //     pos += pos.x > dest.x ? new int2(-1, 0) : new int2(1, 0);

            //     waypoints.Add(new Waypoint { pos = pos });

            //     if (pos.y != dest.y)
            //     {
            //         pos += pos.y > dest.y ? new int2(0, -1) : new int2(0, 1);
            //         waypoints.Add(new Waypoint { pos = pos });
            //     }

            //     if (i > maxIterations) throw new();
            //     i++;
            // }
            // i = 0;
            // while (pos.y != dest.y)
            // {
            //     pos += pos.y > dest.y ? new int2(0, -1) : new int2(0, 1);
            //     waypoints.Add(new Waypoint { pos = pos });

            //     if (i > maxIterations) throw new();
            //     i++;
            // }
        }
    }
}