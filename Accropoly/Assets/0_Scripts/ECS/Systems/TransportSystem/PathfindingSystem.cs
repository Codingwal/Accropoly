using System.Linq;
using Components;
using Components.WaypointComponents;
using Tags;
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

            // Prepare traveller data for serialization (waypoints can't be serialized directly because entity ids might differ after restarting)
            if (SystemAPI.HasSingleton<PreSaveGame>())
            {
                Entities.ForEach((Entity entity, ref Traveller traveller) =>
                {
                    TravellerWaypointsSerializable waypoints = new()
                    {
                        waypoints = new(8, Allocator.Persistent, NativeArrayOptions.UninitializedMemory)
                    };
                    foreach (Entity waypoint in traveller.waypoints)
                    {
                        float3 waypointPos = SystemAPI.GetComponent<LocalTransform>(waypoint).Position;
                        waypoints.waypoints.Add(waypointPos);
                    }
                    traveller.waypoints.Dispose();
                    ecb.AddComponent(entity, waypoints);
                }).Schedule();
            }

            if (!SystemAPI.HasSingleton<RunGame>())
                return;

            // Recreate waypoints list from serialization container (can't be serialized directly because entity ids might differ after restarting)
            var waypointsData = SystemAPI.GetSingleton<WaypointsData>();
            if (!waypointsData.waypoints.IsEmpty)
            {
                Entities.ForEach((Entity entity, ref Traveller traveller, ref TravellerWaypointsSerializable waypoints) =>
                {
                    traveller.waypoints = new(8, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                    foreach (float3 waypointPos in waypoints.waypoints)
                    {
                        Entity waypoint = waypointsData.waypoints[waypointPos];
                        traveller.waypoints.Add(waypoint);
                    }
                    waypoints.waypoints.Dispose();
                    ecb.RemoveComponent<TravellerWaypointsSerializable>(entity);
                }).Run();
            }

            var utility = new PathfindingUtility()
            {
                transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(),
                connectionsLookup = SystemAPI.GetComponentLookup<Connections>(),
                waypointLookup = SystemAPI.GetComponentLookup<Waypoint>(),
                waypointsData = waypointsData,
            };

            // Handle objects requesting a path
            Entities.WithAll<WantsToTravel>().ForEach((Entity entity, ref Traveller traveller, in LocalTransform transform) =>
            {
                traveller.Reset();

                float3 dest = new(traveller.destination.x * 2, 0.8f, traveller.destination.y * 2);

                if (utility.FindPath(ref traveller.waypoints, transform.Position, dest))
                {
                    ecb.SetComponentEnabled<Travelling>(entity, true);
                }
                else Debug.LogWarning($"Couldn't find path from {transform.Position} to {dest}!");
                ecb.SetComponentEnabled<WantsToTravel>(entity, false);
            }).Schedule();
        }
    }

    public partial struct PathfindingUtility
    {
        [NativeDisableContainerSafetyRestriction] public ComponentLookup<LocalTransform> transformLookup;
        public ComponentLookup<Connections> connectionsLookup;
        public ComponentLookup<Waypoint> waypointLookup;
        public WaypointsData waypointsData;

        /// <remarks>Returns -1 if no path is found</remarks>
        public float CalculateTravelTime(int2 startTile, int2 destTile)
        {
            float3 start = new(startTile.x * 2, 0.8f, startTile.y * 2);
            float3 dest = new(destTile.x * 2, 0.8f, destTile.y * 2);

            UnsafeList<Entity> path = new(10, Allocator.TempJob);
            float travelTime = 0;

            if (FindPath(ref path, start, dest))
            {
                for (int i = 1; i < path.Length; i++)
                {
                    float3 posA = transformLookup.GetRefRO(path[i - 1]).ValueRO.Position;
                    float3 posB = transformLookup.GetRefRO(path[i]).ValueRO.Position;
                    float distance = math.distance(posA, posB);

                    float speedA = waypointLookup.GetRefRO(path[i - 1]).ValueRO.velocity;
                    float speedB = waypointLookup.GetRefRO(path[i]).ValueRO.velocity;
                    float averageSpeed = (speedA + speedB) * 0.5f;

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
        public bool FindPath(ref UnsafeList<Entity> path, float3 start, float3 dest, TravelObjects useableVehicles = TravelObjects.Standard)
        {
            Debug.Assert(path.IsCreated, "The path list has not been created");
            Debug.Assert(path.IsEmpty, "The path list must be empty");
            Debug.Assert(!start.Equals(dest), $"Start must not equal destination (start and dest are {start})");

            NativeList<(float, NodeToVisit)> openList = new(8, Allocator.TempJob); // (cost, info)
            NativeHashMap<Entity, VisitedNode> closedList = new(8, Allocator.TempJob); // (entity, info)

            NativeList<Direction> directions = new(4, Allocator.TempJob); // Contains the four directions
            Direction.GetDirections(ref directions);

            void Dispose()
            {
                openList.Dispose();
                closedList.Dispose();
                directions.Dispose();
            }

            Debug.Assert(waypointsData.waypoints.TryGetValue(start, out Entity startWaypoint), $"There is no waypoint at the start pos {start}");
            openList.Add((0, new(startWaypoint, Entity.Null)));

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
                    NativeList<Entity> reversedPath = new(Allocator.TempJob);
                    Entity current = node.entity;
                    while (current != Entity.Null)
                    {
                        reversedPath.Add(current);
                        current = closedList[current].previous;
                    }

                    // Reverse path (can't use linq with unmanaged stuff / burst (?))
                    for (int i = reversedPath.Length - 1; i >= 0; i--)
                        path.Add(reversedPath[i]);
                    reversedPath.Dispose();

                    Dispose();
                    return true;
                }

                // Get neighbours
                RefRO<Connections> connections = connectionsLookup.GetRefRO(node.entity);

                // Add neighbours to openList (if they are valid)
                foreach (Entity next in connections.ValueRO.next)
                {
                    if (next == Entity.Null) continue;

                    var waypointData = waypointLookup.GetRefRO(next);

                    // Check if this waypoint is accessible
                    if ((waypointData.ValueRO.allowedObjects & useableVehicles) == TravelObjects.None)
                        continue;

                    float speed = waypointData.ValueRO.velocity;
                    float3 nextPos = transformLookup.GetRefRO(next).ValueRO.Position;
                    openList.Add((CalculateCost(nextPos, pos, cost, dest, speed), new NodeToVisit(next, node.entity)));
                }
                directions.Clear();

                if (iteration > 1000) throw new();
                iteration++;
            }
            Dispose();
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
        private static bool IsAdjacent(int2 a, int2 b)
        {
            return ManhattanDistance(a, b) == 1;
        }
        private static float CalculateCost(float3 pos, float3 previousPos, float previousCost, float3 dest, float tileSpeed)
        {
            // prevCost + (movementCost + dist(currPos) - dist(prevPos))
            return previousCost + 1 / tileSpeed + ManhattanDistance(pos, dest) - ManhattanDistance(previousPos, dest);
        }
        private static float ManhattanDistance(int2 pos, int2 dest)
        {
            int2 v = math.abs(dest - pos);
            return v.x + v.y;
        }
        private static float ManhattanDistance(float3 pos, float3 dest)
        {
            float3 v = math.abs(dest - pos);
            return v.x + v.y + v.z;
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