using Components;
using Components.WaypointComponents;
using Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    [UpdateAfter(typeof(ValidatePathSystem))]
    public partial struct TravelProgressSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WaypointsData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new UpdateTravelProgressJob()
            {
                waypointsData = SystemAPI.GetSingleton<WaypointsData>(),
                junctionLookup = SystemAPI.GetComponentLookup<Junction>(),
                travellingLookup = SystemAPI.GetComponentLookup<Travelling>(),
                connectionsLookup = SystemAPI.GetBufferLookup<Connection>(isReadOnly: true)
            }.Schedule();
        }

        [BurstCompile]
        [WithAll(typeof(Travelling))]
        private partial struct UpdateTravelProgressJob : IJobEntity
        {
            public WaypointsData waypointsData;
            public ComponentLookup<Junction> junctionLookup;
            public ComponentLookup<Travelling> travellingLookup;
            [ReadOnly] public BufferLookup<Connection> connectionsLookup;
            public void Execute(Entity entity, ref MovementInfo movementInfo, ref CurveFollower curveFollower, ref LocalTransform transform, in DynamicBuffer<PathElement> path)
            {
                // Start with second waypoint as target (object starts at first waypoint)
                if (movementInfo.nextWaypointIndex == 0)
                {
                    movementInfo.nextWaypointIndex++;
                    UpdateInfo(ref movementInfo, ref curveFollower, in path);
                }

                if (movementInfo.nextWaypointIndex == path.Length - 1) // Targeting last waypoint
                {
                    transform.Position = movementInfo.nextWaypoint; // Teleport to destination
                    travellingLookup.SetComponentEnabled(entity, false); // Set Travelling tag to false
                    DeregisterAtWaypoint(movementInfo.nextWaypoint);

                    // Reset / invalidate data
                    movementInfo.nextWaypointIndex = 0;
                    movementInfo.nextWaypoint = new(float.NaN);
                    curveFollower.curve = new(float.NaN, float.NaN, float.NaN);
                    curveFollower.timeAlongCurve = float.NaN;
                }

                if (math.distancesq(transform.Position, movementInfo.nextWaypoint) < math.square(0.1f)) // Reached waypoint?
                {
                    DeregisterAtWaypoint(movementInfo.nextWaypoint);

                    // Update targeted waypoint
                    movementInfo.nextWaypointIndex++;
                    UpdateInfo(ref movementInfo, ref curveFollower, in path);

                }
            }

            private void UpdateInfo(ref MovementInfo movementInfo, ref CurveFollower curveFollower, in DynamicBuffer<PathElement> path)
            {
                // Update nextWaypoint (nextWaypointIndex has already been updated)
                movementInfo.nextWaypoint = path[movementInfo.nextWaypointIndex].waypoint;

                // Get information about the previous waypoint (important for the curve)
                float3 prevWaypoint = path[movementInfo.nextWaypointIndex - 1].waypoint;
                Entity prevWaypointEntity = GetEntity(prevWaypoint);
                var connections = connectionsLookup[prevWaypointEntity];

                // Find the connection we are taking to get the connection data
                Connection connection = new();
                foreach (Connection _connection in connections)
                {
                    if (_connection.nextWaypoint.Equals(movementInfo.nextWaypoint))
                        connection = _connection;
                }

                // Update curveFollower
                curveFollower.curve = new(prevWaypoint, connection.controlPoint, movementInfo.nextWaypoint);
                curveFollower.timeAlongCurve = 0;

                RegisterAtWaypoint(movementInfo.nextWaypoint);
            }

            private void RegisterAtWaypoint(float3 waypoint)
            {
                Entity entity = GetEntity(waypoint);
                if (junctionLookup.TryGetRefRW(entity, out var junctionRef))
                    junctionRef.ValueRW.registeredObjects++;
            }
            private void DeregisterAtWaypoint(float3 waypoint)
            {
                Entity entity = GetEntity(waypoint);
                if (junctionLookup.TryGetRefRW(entity, out var junctionRef))
                    junctionRef.ValueRW.registeredObjects--;
            }
            private Entity GetEntity(float3 pos)
            {
                return waypointsData.waypoints[pos];
            }
        }
    }
}