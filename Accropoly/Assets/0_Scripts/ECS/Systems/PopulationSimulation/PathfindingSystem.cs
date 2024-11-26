using Components;
using Tags;
using Unity.Entities;
using UnityEngine;

namespace Systems
{
    public partial class PathfindingSystem : SystemBase
    {
        private const int waypointCount = 4;
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

            Entities.WithDisabled<Travelling>().ForEach((Entity entity, ref Traveller traveller) =>
            {
                traveller.nextWaypointIndex = 0;

                if (traveller.waypoints.IsCreated)
                    traveller.waypoints.Clear();
                else
                    traveller.waypoints = new(waypointCount, Unity.Collections.Allocator.Persistent, Unity.Collections.NativeArrayOptions.UninitializedMemory);

                for (int i = 0; i < waypointCount; i++)
                {
                    traveller.waypoints.AddNoResize(new Waypoint
                    {
                        pos = rnd.NextFloat2(0, 20)
                    });
                }

                ecb.SetComponentEnabled<Travelling>(entity, true);
            }).Schedule();
        }
    }
}
