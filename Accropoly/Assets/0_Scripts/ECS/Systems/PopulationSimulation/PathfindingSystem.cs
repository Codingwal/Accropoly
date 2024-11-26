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
        private static Unity.Mathematics.Random rnd;
        protected override void OnCreate()
        {
            RequireForUpdate<Traveller>();
            RequireForUpdate<RunGame>();
            rnd = new(1);
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            Entities.WithAll<WantsToTravel>().ForEach((Entity entity, ref Traveller traveller, in LocalTransform transform) =>
            {
                traveller.nextWaypointIndex = 0;

                if (traveller.waypoints.IsCreated)
                    traveller.waypoints.Clear();
                else
                    traveller.waypoints = new(8, Unity.Collections.Allocator.Persistent, Unity.Collections.NativeArrayOptions.UninitializedMemory);

                FindPath(ref traveller.waypoints, (int2)math.round(transform.Position.xz), traveller.destination * 2);

                ecb.SetComponentEnabled<Travelling>(entity, true);
                ecb.SetComponentEnabled<WantsToTravel>(entity, false);
            }).Schedule();
        }
        private static void FindPath(ref UnsafeList<Waypoint> waypoints, int2 start, int2 dest)
        {
            // Both start and dest must tile centers
            Debug.Assert((start % 2).Equals(0), "Both start and dest must tile centers");
            Debug.Assert((dest % 2).Equals(0), "Both start and dest must tile centers");
            Debug.Assert(!start.Equals(dest), "The start must not equal the destination");


            const int maxIterations = 1000;
            int i = 0;

            int2 pos = start;
            while (pos.x != dest.x)
            {
                pos += pos.x > dest.x ? new int2(-2, 0) : new int2(2, 0);

                waypoints.Add(new Waypoint { pos = pos });

                if (pos.y != dest.y)
                {
                    pos += pos.y > dest.y ? new int2(0, -2) : new int2(0, 2);
                    waypoints.Add(new Waypoint { pos = pos });
                }

                if (i > maxIterations) throw new();
                i++;
            }
            i = 0;
            while (pos.y != dest.y)
            {
                pos += pos.y > dest.y ? new int2(0, -2) : new int2(0, 2);
                waypoints.Add(new Waypoint { pos = pos });

                if (i > maxIterations) throw new();
                i++;
            }
        }
    }
}
