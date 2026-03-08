using Components;
using Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    /// <summary>
    /// Move people that are currently travelling (Travelling tag)
    /// Does not calculate the path, only moves the person along the waypoints managed by PathfindingSystem
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class MovementSystem : SystemBase
    {
        protected override void OnUpdate()
        {

        }
        public void DrawGizmos(bool debugPath, bool debugCurrentTarget)
        {
            if (debugPath)
            {
                Gizmos.color = Color.green;
                foreach (var (movementInfo, path) in SystemAPI.Query<RefRO<MovementInfo>, DynamicBuffer<PathElement>>().WithAll<Travelling>())
                {
                    for (int i = movementInfo.ValueRO.nextWaypointIndex + 1; i < path.Length; i++)
                    {
                        float3 from = path[i - 1].waypoint;
                        float3 to = path[i].waypoint;
                        Gizmos.DrawLine(from, to);
                    }

                }
            }

            if (debugCurrentTarget)
            {
                Gizmos.color = Color.yellow;
                foreach (var (movementInfo, transform) in SystemAPI.Query<RefRO<MovementInfo>, RefRO<LocalTransform>>().WithAll<Travelling>())
                {
                    float3 from = transform.ValueRO.Position;
                    float3 to = movementInfo.ValueRO.nextWaypoint;
                    Gizmos.DrawLine(from, to);
                }
            }
        }
    }
}