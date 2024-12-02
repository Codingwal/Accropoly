using System;
using System.Linq;
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
                traveller.nextWaypointIndex = 1; // waypoints[0] is start

                if (traveller.waypoints.IsCreated)
                    traveller.waypoints.Clear();
                else
                    traveller.waypoints = new(8, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

                if (FindPath(ref traveller.waypoints, (int2)math.round(transform.Position.xz) / 2, traveller.destination, entityGrid))
                {
                    ecb.SetComponentEnabled<Travelling>(entity, true);
                }
                ecb.SetComponentEnabled<WantsToTravel>(entity, false);
            }).Schedule();
        }
        public static float CalculateTravelTime(int2 start, int2 dest, in DynamicBuffer<EntityBufferElement> entityGrid)
        {
            UnsafeList<Waypoint> path = new(10, Allocator.TempJob);
            float travelTime = 0;

            if (FindPath(ref path, start, dest, entityGrid))
            {
                for (int i = 1; i < path.Length - 1; i++)
                {
                    Entity tileEntity = TileGridUtility.GetTile(path[i].pos, entityGrid);
                    Debug.Assert(transportTilesLookup.HasComponent(tileEntity), $"Tile {path[i].pos} should be a transport tile");
                    var transportTile = transportTilesLookup.GetRefRO(tileEntity);
                    travelTime += 20 / transportTile.ValueRO.speed / TransportTileAspect.travelSecondsPerSecond; // tileSize [in m] / speed * secondsPerTravelSecond
                }
            }
            else travelTime = -1; // If there is no path

            path.Dispose();
            Debug.Log(travelTime);
            return travelTime;
        }
        /// <summary>Finds the shortest path using A* pathfinding from start to dest and stores it in waypoints.</summary>
        /// <param name="buffer">The buffer containing the tile grid</param>
        /// <returns>Returns true if a path was found</returns>
        private static bool FindPath(ref UnsafeList<Waypoint> waypoints, int2 start, int2 dest, in DynamicBuffer<EntityBufferElement> entityGrid)
        {
            Debug.Assert(!start.Equals(dest), $"Start must not equal destination (start and dest are {start})");
            Debug.Assert(waypoints.IsCreated, "The UnsafeList<Waypoint> has not been created");

            NativeList<(float, NodeToVisit)> openList = new(8, Allocator.TempJob) { (0, new(start, -1)) };
            NativeHashMap<int2, VisitedNode> closedList = new(8, Allocator.TempJob);

            int iteration = 0;

            // AStar
            while (openList.Length != 0)
            {
                var (cost, node) = PopCheapest(openList);
                if (closedList.ContainsKey(node.pos)) continue; // Skip already visited nodes
                closedList.Add(node.pos, new(node.previous));
                foreach (Direction dir in Direction.GetDirections())
                {
                    int2 neighbourPos = node.pos + dir.DirectionVec;
                    if (neighbourPos.Equals(dest))
                    {
                        // Get path
                        NativeList<Waypoint> reversedPath = new(Allocator.TempJob);
                        int2 currentPos = node.pos;
                        while (!currentPos.Equals(start))
                        {
                            reversedPath.Add(new(currentPos));
                            currentPos = closedList[currentPos].previous;
                        }

                        // Reverse path
                        waypoints.Add(new(start));
                        for (int i = reversedPath.Length - 1; i >= 0; i--)
                            waypoints.Add(reversedPath[i]);
                        reversedPath.Dispose();

                        waypoints.Add(new(dest));
                        openList.Dispose();
                        closedList.Dispose();
                        return true;
                    }
                    if (!TileGridUtility.TryGetTile(neighbourPos, entityGrid, out Entity entity)) continue; // Skip positions outside of the map
                    if (!transportTilesLookup.TryGetComponent(entity, out TransportTile transportTile)) continue; // Skip non-street tiles

                    openList.Add((CalculateCost(neighbourPos, node.pos, cost, dest, transportTile.speed), new(neighbourPos, node.pos)));
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
        private static float CalculateCost(int2 pos, int2 previousPos, float previousCost, int2 dest, float tileSpeed)
        {
            return previousCost + 1 / tileSpeed + ManhattanDistance(pos, dest) - ManhattanDistance(previousPos, dest);
        }
        private static float ManhattanDistance(int2 pos, int2 dest)
        {
            int2 v = dest - pos;
            return v.x + v.y;
        }
        private struct VisitedNode
        {
            public int2 previous;
            public VisitedNode(int2 previous)
            {
                this.previous = previous;
            }
        }
        private struct NodeToVisit
        {
            public int2 pos;
            public int2 previous;
            public NodeToVisit(int2 pos, int2 previous)
            {
                this.pos = pos;
                this.previous = previous;
            }
        }
    }
}