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
        private static TransportTileAspect.Lookup transportTileAspectLookup;
        protected override void OnCreate()
        {
            RequireForUpdate<Traveller>();
            RequireForUpdate<RunGame>();

            transportTilesLookup = GetComponentLookup<TransportTile>(isReadOnly: true);
            transportTileAspectLookup = new(ref CheckedStateRef);
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            var entityGrid = SystemAPI.GetBuffer<EntityBufferElement>(SystemAPI.GetSingletonEntity<EntityGridHolder>());

            transportTilesLookup.Update(this);
            transportTileAspectLookup.Update(ref CheckedStateRef);

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
                else Debug.LogWarning($"Couldn't find path from {(int2)math.round(transform.Position.xz) / 2} to {traveller.destination}!");
                ecb.SetComponentEnabled<WantsToTravel>(entity, false);
            }).Schedule();
        }

        /// <remarks>Returns -1 if no path is found</remarks>
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
            return travelTime;
        }
        /// <summary>Finds the shortest path using A* pathfinding from start to dest and stores it in waypoints.</summary>
        /// <param name="buffer">The buffer containing the tile grid</param>
        /// <returns>Returns true if a path was found</returns>
        private static bool FindPath(ref UnsafeList<Waypoint> waypoints, int2 start, int2 dest, in DynamicBuffer<EntityBufferElement> entityGrid)
        {
            Debug.Assert(!start.Equals(dest), $"Start must not equal destination (start and dest are {start})");
            Debug.Assert(waypoints.IsCreated, "The UnsafeList<Waypoint> has not been created");

            NativeList<(float, NodeToVisit)> openList = new(8, Allocator.TempJob) { (0, new(start, -1)) }; // (cost, info)
            NativeHashMap<int2, VisitedNode> closedList = new(8, Allocator.TempJob); // (pos, info)

            NativeList<Direction> directions = new(4, Allocator.TempJob); // Create here to improve performance

            void Dispose()
            {
                openList.Dispose();
                closedList.Dispose();
                directions.Dispose();
            }

            int iteration = 0;

            bool IsAdjacent(int2 a, int2 b)
            {
                int2 v = math.abs(a - b);
                return v.x + v.y == 1; // Check if the manhattan distance is 1
            }

            // AStar
            while (openList.Length != 0)
            {
                var (cost, node) = PopCheapest(openList);
                if (closedList.ContainsKey(node.pos)) continue; // Skip already visited nodes
                closedList.Add(node.pos, new(node.previous));

                // If this tile is next to the destination, create waypoint list and return
                if (IsAdjacent(node.pos, dest))
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
                    Dispose();
                    return true;
                }

                // Get neighbours
                if (node.pos.Equals(start))
                {
                    foreach (Direction dir in Direction.GetDirections())
                        directions.Add(dir);
                }
                else
                {
                    Entity tile = TileGridUtility.GetTile(node.pos, entityGrid);
                    var transportTileAspect = transportTileAspectLookup[tile];
                    transportTileAspect.GetDirections(ref directions);
                }

                // Add neighbours to openList (if they are valid)
                foreach (Direction dir in directions)
                {
                    int2 neighbourPos = node.pos + dir.DirectionVec;

                    if (!TileGridUtility.TryGetTile(neighbourPos, entityGrid, out Entity neighbourEntity)) continue; // Skip positions outside of the map
                    if (!transportTilesLookup.TryGetComponent(neighbourEntity, out TransportTile transportTile)) continue; // Skip non-street tiles

                    openList.Add((CalculateCost(neighbourPos, node.pos, cost, dest, transportTile.speed), new(neighbourPos, node.pos)));
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
        private static float CalculateCost(int2 pos, int2 previousPos, float previousCost, int2 dest, float tileSpeed)
        {
            // prevCost + (movementCost + dist(currPos) - dist(prevPos))
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