using System;
using Components;
using Tags;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
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
        private static ComponentLookup<TransportTile> transportTilesLookup;
        protected override void OnCreate()
        {
            RequireForUpdate<Traveller>();
            RequireForUpdate<RunGame>();

            transportTilesLookup = GetComponentLookup<TransportTile>(isReadOnly: true);
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            var entityGrid = SystemAPI.GetBuffer<EntityBufferElement>(SystemAPI.GetSingletonEntity<EntityGridHolder>());

            transportTilesLookup.Update(this);

            Entities.WithAll<WantsToTravel>().ForEach((Entity entity, ref Traveller traveller, in LocalTransform transform) =>
            {
                if (traveller.waypoints.IsCreated)
                    traveller.waypoints.Clear();
                else
                    traveller.waypoints = new(8, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

                if (FindPath(ref traveller.waypoints, (int2)math.round(transform.Position.xz / 2), traveller.destination, entityGrid))
                {
                    traveller.nextWaypointIndex = 1; // waypoints[0] is start
                    traveller.maxAcceleration = 2.7f;
                    traveller.velocity = float3.zero;
                    ecb.SetComponentEnabled<Travelling>(entity, true);
                }
                else Debug.LogWarning($"Couldn't find path from {(int2)math.round(transform.Position.xz) / 2} to {traveller.destination}!");
                ecb.SetComponentEnabled<WantsToTravel>(entity, false);
            }).WithoutBurst().Schedule();
        }

        /// <remarks>Returns -1 if no path is found</remarks>
        public static float CalculateTravelTime(int2 start, int2 dest, in DynamicBuffer<EntityBufferElement> entityGrid)
        {
            UnsafeList<float3> path = new(10, Allocator.TempJob);
            float travelTime = 0;

            if (FindPath(ref path, start, dest, entityGrid))
            {
                travelTime = 1;
            }
            else travelTime = -1; // If there is no path

            path.Dispose();
            return travelTime;
        }
        /// <summary>Finds the shortest path using A* pathfinding from start to dest and stores it in waypoints.</summary>
        /// <returns>Returns true if a path was found</returns>
        private static bool FindPath(ref UnsafeList<float3> path, int2 startTile, int2 destTile, in DynamicBuffer<EntityBufferElement> entityGrid)
        {
            Debug.Assert(!startTile.Equals(destTile), $"Start must not equal destination (start and dest are {startTile})");
            Debug.Assert(path.IsCreated, "The UnsafeList<Waypoint> has not been created");

            var waypoints = WaypointSystem.waypoints.AsReadOnly();
            float3 start = new(startTile.x * 2, 0.8f, startTile.y * 2);
            float3 dest = new(destTile.x * 2, 0.8f, destTile.y * 2);

            NativeList<(float, NodeToVisit)> openList = new(8, Allocator.TempJob); // (cost, info)
            NativeHashMap<float3, VisitedNode> closedList = new(8, Allocator.TempJob); // (pos, info)

            NativeList<Direction> directions = new(4, Allocator.TempJob); // Contains the four directions
            Direction.GetDirections(ref directions);

            void Dispose()
            {
                openList.Dispose();
                closedList.Dispose();
                directions.Dispose();
            }

            // The journey can start on all waypoints on adjacent street tiles
            foreach (Direction dir in directions)
            {
                if (!TileGridUtility.TryGetTile(startTile + dir.DirectionVec, entityGrid, out Entity tile))
                    continue;
                if (!transportTilesLookup.TryGetComponent(tile, out var transportTile))
                    continue;

                for (int i = 0; i < transportTile.waypoints.Size; i++)
                {
                    float3 waypoint = transportTile.waypoints[i];
                    if (math.isnan(waypoint.x)) continue;
                    openList.Add((0, new(waypoint, start)));
                }
            }

            int iteration = 0;

            // AStar
            while (openList.Length != 0)
            {
                var (cost, node) = PopCheapest(openList);
                if (closedList.ContainsKey(node.pos)) continue; // Skip already visited nodes
                closedList.Add(node.pos, new(node.previous));

                // If this tile is next to the destination, create waypoint list and return
                if (IsAdjacent((int2)math.round(node.pos.xz / 2), destTile))
                {
                    // Get path
                    NativeList<float3> reversedPath = new(Allocator.TempJob);
                    float3 currentPos = node.pos;
                    while (!currentPos.Equals(start))
                    {
                        reversedPath.Add(currentPos);
                        currentPos = closedList[currentPos].previous;
                    }

                    // Reverse path
                    path.Add(start);
                    for (int i = reversedPath.Length - 1; i >= 0; i--)
                        path.Add(reversedPath[i]);
                    reversedPath.Dispose();

                    path.Add(dest);
                    Dispose();
                    return true;
                }

                // Get neighbours
                Waypoint waypoint = waypoints[node.pos];


                // Add neighbours to openList (if they are valid)
                for (int i = 0; i < waypoint.next.Size; i++)
                {
                    float3 next = waypoint.next[i];
                    if (math.isnan(next.x)) continue;

                    float speed = transportTilesLookup.GetRefRO(GetTile(next, entityGrid)).ValueRO.speed;
                    openList.Add((CalculateCost(next, node.pos, cost, dest, speed), new(next, node.pos)));
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
        private static Entity GetTile(float3 pos, in DynamicBuffer<EntityBufferElement> entityGrid)
        {
            int2 tilePos = (int2)math.round(pos.xz / 2);
            return TileGridUtility.GetTile(tilePos, entityGrid);
        }
        private struct VisitedNode
        {
            public float3 previous;
            public VisitedNode(float3 previous)
            {
                this.previous = previous;
            }
        }
        private struct NodeToVisit
        {
            public float3 pos;
            public float3 previous;
            public NodeToVisit(float3 pos, float3 previous)
            {
                this.pos = pos;
                this.previous = previous;
            }
        }
    }
}