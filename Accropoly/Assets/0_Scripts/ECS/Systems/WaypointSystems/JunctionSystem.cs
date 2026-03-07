using Components;
using Components.WaypointComponents;
using Tags;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using JunctionData = Components.WaypointComponents.Junction.JunctionData;

namespace Systems
{
    public partial class JunctionSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<TransportTile>();
        }
        protected override void OnUpdate()
        {
            new UpdateJunctionsJob()
            {
                deltaTime = SystemAPI.GetSingleton<GameInfo>().deltaTime / MovementSystem.gameSecondsPerMovementSecond,
                junctionLookup = SystemAPI.GetComponentLookup<Junction>()
            }.Schedule();
        }

        [BurstCompile]
        private partial struct UpdateJunctionsJob : IJobEntity
        {
            public float deltaTime;
            public ComponentLookup<Junction> junctionLookup;
            public void Execute(ref TransportTile transportTile, in ConnectingTile connectingTile)
            {
                int index = connectingTile.GetIndex();
                if (!(index == ConnectingTile.tJunction || index == ConnectingTile.junction))
                    return;

                bool priorityObject = false; // Is there an object others should give way to?

                // Collect data (iterate over waypoints)
                foreach (Entity waypoint in transportTile.waypoints)
                {
                    if (waypoint == Entity.Null) continue;

                    // Skip waypoints that are not part of the junction
                    if (!junctionLookup.HasComponent(waypoint))
                        continue;

                    var junction = junctionLookup.GetRefRW(waypoint);
                    JunctionData junctionData = junction.ValueRO.junctionData;

                    if (junctionData == JunctionData.None) continue;
                    Debug.Assert(junctionData != JunctionData.Default);

                    if (junctionData == JunctionData.Priority)
                    {
                        if (junction.ValueRO.registeredObjects > 0)
                        {
                            priorityObject = true;
                            break;
                        }
                    }
                }

                if (priorityObject)
                    transportTile.timer = 0.08f;

                if (!priorityObject && transportTile.timer > 0) // There has been a priority object a few moments earlier
                {
                    priorityObject = true; // Block others until timer expired
                    transportTile.timer -= deltaTime;
                    transportTile.timer = math.clamp(transportTile.timer, 0, float.MaxValue);
                }

                // Update waypoints
                foreach (Entity waypoint in transportTile.waypoints)
                {
                    if (waypoint == Entity.Null) continue;

                    // Skip waypoints that are not part of the junction
                    if (!junctionLookup.HasComponent(waypoint))
                        continue;

                    var junction = junctionLookup.GetRefRW(waypoint);
                    JunctionData junctionData = junction.ValueRO.junctionData;

                    if (junctionData == JunctionData.None) continue;
                    Debug.Assert(junctionData != JunctionData.Default);

                    if (junctionData == JunctionData.GiveWay)
                    {
                        junction.ValueRW.stop = priorityObject; // Stop if there is a priority object
                    }
                }
            }
        }
    }
}