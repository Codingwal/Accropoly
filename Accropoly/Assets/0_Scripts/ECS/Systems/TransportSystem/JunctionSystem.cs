using Components;
using Tags;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using JunctionData = Waypoint.JunctionData;

namespace Systems
{
    public partial class JunctionSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<TransportTile>();
            RequireForUpdate<RunGame>();
        }
        protected override void OnUpdate()
        {
            new UpdateJunctionsJob()
            {
                waypointsData = SystemAPI.GetSingletonRW<WaypointsData>(),
                deltaTime = SystemAPI.GetSingleton<GameInfo>().deltaTime / MovementSystem.gameSecondsPerMovementSecond
            }.Schedule();
        }

        [BurstCompile]
        private partial struct UpdateJunctionsJob : IJobEntity
        {
            [NativeDisableUnsafePtrRestriction]
            public RefRW<WaypointsData> waypointsData;
            public float deltaTime;
            public void Execute(ref TransportTile transportTile, in ConnectingTile connectingTile)
            {
                int index = connectingTile.GetIndex();
                if (!(index == ConnectingTile.tJunction || index == ConnectingTile.junction))
                    return;

                bool priorityObject = false;

                // Collect data
                foreach (float3 waypointPos in transportTile.waypoints)
                {
                    if (math.isnan(waypointPos.x)) continue;
                    Waypoint waypoint = waypointsData.ValueRO.waypoints[waypointPos];
                    if (waypoint.junctionData == JunctionData.None) continue;
                    Debug.Assert(waypoint.junctionData != JunctionData.Default);

                    if (waypoint.junctionData == JunctionData.Priority)
                    {
                        if (waypoint.registeredObjects > 0)
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
                foreach (float3 waypointPos in transportTile.waypoints)
                {
                    if (math.isnan(waypointPos.x)) continue;
                    Waypoint waypoint = waypointsData.ValueRO.waypoints[waypointPos];
                    if (waypoint.junctionData == JunctionData.None) continue;
                    Debug.Assert(waypoint.junctionData != JunctionData.Default);

                    if (waypoint.junctionData == JunctionData.GiveWay)
                    {
                        waypoint.stop = priorityObject; // Stop if there is a priority object
                    }
                    waypointsData.ValueRW.waypoints[waypointPos] = waypoint;
                }
            }
        }
    }
}