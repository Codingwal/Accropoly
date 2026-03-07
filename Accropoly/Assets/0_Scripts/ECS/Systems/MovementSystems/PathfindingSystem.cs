using System.Linq;
using Components;
using Components.WaypointComponents;
using Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    /// <summary>
    /// Calculates the path for people with the WantsToTravel tag (from current pos to traveller.destination)
    /// After the path is calculated and stored in traveller.waypoints, the person is set to travelling (Travelling tag)
    /// </summary>
    public partial class PathfindingSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<Traveller>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            var utility = new PathfindingUtility()
            {
                transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(isReadOnly: true),
                connectionsLookup = SystemAPI.GetBufferLookup<Connection>(isReadOnly: true),
                waypointLookup = SystemAPI.GetComponentLookup<Waypoint>(isReadOnly: true),
                waypointsData = SystemAPI.GetSingleton<WaypointsData>(),
            };

            // Handle objects requesting a path
            new CalculatePathsJob
            {
                ecb = ecb,
                utility = utility,
            }.Schedule();
        }

        [BurstCompile]
        [WithAll(typeof(WantsToTravel))]
        private partial struct CalculatePathsJob : IJobEntity
        {
            public EntityCommandBuffer ecb;
            public PathfindingUtility utility;
            public void Execute(Entity entity, ref Traveller traveller, in LocalTransform transform, ref DynamicBuffer<PathElement> path)
            {
                float3 dest = new(traveller.destination.x * 2, 0.8f, traveller.destination.y * 2);

                UnsafeList<float3> pathList = new(10, Allocator.TempJob);

                if (!utility.FindPath(ref pathList, transform.Position, dest))
                    Debug.LogWarning($"Couldn't find path from {transform.Position} to {dest}!");

                path.Clear();
                foreach (float3 waypoint in pathList)
                {
                    path.Add(waypoint);
                }

                pathList.Dispose();

                ecb.SetComponentEnabled<Travelling>(entity, true);
                ecb.SetComponentEnabled<WantsToTravel>(entity, false);
            }
        }
    }

    public struct PathfindingUtility
    {
        [NativeDisableContainerSafetyRestriction]
        [ReadOnly] public ComponentLookup<LocalTransform> transformLookup;
        [ReadOnly] public BufferLookup<Connection> connectionsLookup;
        [ReadOnly] public ComponentLookup<Waypoint> waypointLookup;
        [ReadOnly] public WaypointsData waypointsData;

        /// <remarks>Returns -1 if no path is found</remarks>
        public float CalculateTravelTime(int2 startTile, int2 destTile)
        {
            float3 start = new(startTile.x * 2, 0.8f, startTile.y * 2);
            float3 dest = new(destTile.x * 2, 0.8f, destTile.y * 2);

            UnsafeList<float3> path = new(10, Allocator.TempJob);
            float travelTime = 0;

            if (FindPath(ref path, start, dest))
            {
                for (int i = 1; i < path.Length; i++)
                {
                    float distance = math.distance(path[i - 1], path[i]);

                    // float speedA = waypointLookup.GetRefRO(path[i - 1]).ValueRO.velocity;
                    // float speedB = waypointLookup.GetRefRO(path[i]).ValueRO.velocity;
                    // float averageSpeed = (speedA + speedB) * 0.5f;
                    float averageSpeed = 8;

                    travelTime += distance / averageSpeed * MovementSystem.gameSecondsPerMovementSecond;
                }
            }
            else travelTime = -1; // If there is no path

            path.Dispose();
            return travelTime;
        }

        /// <summary>Finds the shortest path using A* pathfinding from start to dest and stores it in waypoints.</summary>
        /// <remarks>The path does not include start and destination</remarks>
        /// <returns>Returns true if a path was found</returns>
        public bool FindPath(ref UnsafeList<float3> path, float3 start, float3 dest, TravelObjects useableVehicles = TravelObjects.Standard)
        {
            Debug.Assert(path.IsCreated, "The path list has not been created");
            Debug.Assert(path.IsEmpty, "The path list must be empty");
            Debug.Assert(!start.Equals(dest), $"Start must not equal destination (start and dest are {start})");
            Debug.Assert(waypointsData.waypoints.ContainsKey(start), $"No waypoint at start pos {start}");
            Debug.Assert(waypointsData.waypoints.ContainsKey(dest), $"No waypoint at destination pos {dest}");

            NativeList<(float, NodeToVisit)> openList = new(8, Allocator.TempJob); // (cost, info)
            NativeHashMap<Entity, VisitedNode> closedList = new(8, Allocator.TempJob); // (entity, info)

            openList.Add((0, new(waypointsData.waypoints[start], Entity.Null)));

            int iteration = 0;

            // AStar
            while (openList.Length != 0)
            {
                var (cost, node) = PopCheapest(openList);
                if (closedList.ContainsKey(node.entity)) continue; // Skip already visited nodes
                closedList.Add(node.entity, new(node.previous));

                float3 pos = transformLookup.GetRefRO(node.entity).ValueRO.Position;

                // If this tile is the destination, create waypoint list and return
                if (pos.Equals(dest))
                {
                    // Get path
                    NativeList<float3> reversedPath = new(Allocator.TempJob);
                    Entity current = node.entity;
                    while (current != Entity.Null)
                    {
                        reversedPath.Add(transformLookup.GetRefRO(current).ValueRO.Position);
                        current = closedList[current].previous;
                    }

                    // Reverse path (can't use linq with unmanaged stuff / burst (?))
                    for (int i = reversedPath.Length - 1; i >= 0; i--)
                        path.Add(reversedPath[i]);
                    reversedPath.Dispose();
                    openList.Dispose();
                    closedList.Dispose();
                    return true;
                }

                // Get neighbours
                var connections = connectionsLookup[node.entity];

                // Add neighbours to openList (if they are valid)
                foreach (Connection connection in connections)
                {
                    if (!waypointsData.waypoints.TryGetValue(connection.nextWaypoint, out var nextEntity))
                        continue;

                    var waypointData = waypointLookup.GetRefRO(nextEntity);

                    // Check if this waypoint is accessible
                    if ((waypointData.ValueRO.allowedObjects & useableVehicles) == TravelObjects.None)
                        continue;

                    float speed = waypointData.ValueRO.velocity;
                    float3 nextPos = transformLookup.GetRefRO(nextEntity).ValueRO.Position;
                    openList.Add((CalculateCost(nextPos, pos, cost, dest, speed), new NodeToVisit(nextEntity, node.entity)));
                }

                if (iteration > 1000) throw new();
                iteration++;
            }
            openList.Dispose();
            closedList.Dispose();
            return false;
        }
        private static (float, NodeToVisit) PopCheapest(in NativeList<(float, NodeToVisit)> openList)
        {
            float lowestCost = float.PositiveInfinity;
            int cheapestIndex = new();
            for (int i = 0; i < openList.Length; i++)
            {
                if (openList[i].Item1 < lowestCost)
                {
                    lowestCost = openList[i].Item1;
                    cheapestIndex = i;
                }
            }
            Debug.Assert(lowestCost != float.PositiveInfinity);

            var cheapestNode = openList[cheapestIndex];
            openList.RemoveAtSwapBack(cheapestIndex);
            return cheapestNode;
        }
        private static float CalculateCost(float3 pos, float3 previousPos, float previousCost, float3 dest, float speed)
        {
            // prevCost + movementCost + estimated cost to dest
            return previousCost + Distance(pos, previousPos) / speed + Distance(pos, dest) / 20;
        }
        private static float Distance(float3 pos, float3 dest)
        {
            return math.distance(pos, dest);
        }
        private struct VisitedNode
        {
            public Entity previous;
            public VisitedNode(Entity previous)
            {
                this.previous = previous;
            }
        }
        private struct NodeToVisit
        {
            public Entity entity;
            public Entity previous;
            public NodeToVisit(Entity entity, Entity previous)
            {
                this.entity = entity;
                this.previous = previous;
            }
        }
    }
}