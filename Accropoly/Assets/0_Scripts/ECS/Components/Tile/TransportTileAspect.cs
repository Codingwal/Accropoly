using Components.WaypointComponents;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using JunctionData = Components.WaypointComponents.Junction.JunctionData;

namespace Components
{
    public readonly partial struct TransportTileAspect : IAspect
    {
        public readonly RefRW<TransportTile> transportTile;
        private readonly RefRO<Tile> tile;
        [Optional] private readonly RefRO<ConnectingTile> connectingTile;
        private readonly RefRO<LocalTransform> transform;

        private const float streetOffset = 0.25f;
        private const float sidewalkOffset = 0.7f;
        public const float defaultHeight = 0.8f;

        struct WaypointData
        {
            public TravelObjects allowedObjects;
            public float velocity;
            public NewWaypoint newWaypointData;
            public JunctionData junctionData;
            public bool entry;
            public bool exit;
            public WaypointData(TravelObjects allowedObjects, float velocity, NewWaypoint newWaypointData,
                JunctionData junctionData = JunctionData.None, bool entry = false, bool exit = false)
            {
                this.allowedObjects = allowedObjects;
                this.velocity = velocity;
                this.newWaypointData = newWaypointData;
                this.junctionData = junctionData;
                this.entry = entry;
                this.exit = exit;
            }
        }

        public readonly void GetPoints(EntityCommandBuffer ecb)
        {
            const float straightSpeed = 17;
            const float curveSpeed = 11;
            const float junctionEntrySpeed = 8;
            const float junctionExitSpeed = 11;
            const float pedestrianSpeed = 3;

            NativeHashMap<float3, WaypointData> waypoints = new(10, Allocator.TempJob);
            NativeList<float3> path = new(Allocator.TempJob);

            // Handle footpath (bitumen for now)
            if (tile.ValueRO.tileType == TileType.Bitumen)
            {
                float3 center = new(0, defaultHeight, 0);
                float3 north = new(0, defaultHeight, 0.95f);
                float3 east = new(0.95f, defaultHeight, 0);
                float3 south = new(0, defaultHeight, -0.95f);
                float3 west = new(-0.95f, defaultHeight, 0);
                waypoints.Add(center, new(TravelObjects.Sidewalk, pedestrianSpeed, new(north, east, south, west)));
                waypoints.Add(north, new(TravelObjects.Sidewalk, pedestrianSpeed, new(center), entry: true, exit: true));
                waypoints.Add(east, new(TravelObjects.Sidewalk, pedestrianSpeed, new(center), entry: true, exit: true));
                waypoints.Add(south, new(TravelObjects.Sidewalk, pedestrianSpeed, new(center), entry: true, exit: true));
                waypoints.Add(west, new(TravelObjects.Sidewalk, pedestrianSpeed, new(center), entry: true, exit: true));
            }
            // Handle buildings with waypoints (habitats, employers, ...)
            else if (!(tile.ValueRO.tileType == TileType.Street || tile.ValueRO.tileType == TileType.CityStreet || tile.ValueRO.tileType == TileType.ForestStreet))
            {
                path.Add(new(0, defaultHeight, 0));
                path.Add(new(0, defaultHeight, 0.95f));
                CreatePath(path, TravelObjects.Sidewalk, pedestrianSpeed, true, waypoints);
            }
            // Handle streets
            else
            {
                Debug.Assert(connectingTile.IsValid);

                int index = connectingTile.ValueRO.GetIndex();

                if (index == ConnectingTile.notConnected)
                {

                }
                else if (index == ConnectingTile.deadEnd)
                {
                    // Street
                    path.Add(new(-streetOffset, defaultHeight, 0.95f));
                    path.Add(new(-streetOffset, defaultHeight, -streetOffset));
                    path.Add(new(streetOffset, defaultHeight, -streetOffset));
                    path.Add(new(streetOffset, defaultHeight, 0.95f));
                    CreatePath(path, TravelObjects.Street, curveSpeed, false, waypoints);
                    path.Clear();

                    // Sidewalks
                    path.Add(new(-sidewalkOffset, defaultHeight, 0.95f)); // northEntry
                    path.Add(new(-sidewalkOffset, defaultHeight, 0)); // entryMiddle
                    path.Add(new(-sidewalkOffset, defaultHeight, -sidewalkOffset)); // centerEntry
                    path.Add(new(0, defaultHeight, -sidewalkOffset)); // centerMiddle
                    path.Add(new(sidewalkOffset, defaultHeight, -sidewalkOffset)); // centerExit
                    path.Add(new(sidewalkOffset, defaultHeight, 0)); // exitMiddle
                    path.Add(new(sidewalkOffset, defaultHeight, 0.95f)); // northExit
                    CreatePath(path, TravelObjects.Sidewalk, pedestrianSpeed, true, waypoints);
                    path.Clear();
                    AddSidewalkExit(Directions.West, pedestrianSpeed, waypoints);
                    AddSidewalkExit(Directions.South, pedestrianSpeed, waypoints);
                    AddSidewalkExit(Directions.East, pedestrianSpeed, waypoints);
                }
                else if (index == ConnectingTile.straight)
                {
                    // north -> south
                    path.Add(new(-streetOffset, defaultHeight, 0.95f)); // northEntry
                    path.Add(new(-streetOffset, defaultHeight, -0.95f)); // southExit
                    CreatePath(path, TravelObjects.Street, straightSpeed, false, waypoints);
                    path.Clear();
                    AddConnection(new(-streetOffset, defaultHeight, -0.95f), new(-sidewalkOffset, defaultHeight, -0.95f), waypoints); // Connect southExit to sidewalk

                    // south -> north
                    path.Add(new(streetOffset, defaultHeight, -0.95f)); // northEntry
                    path.Add(new(streetOffset, defaultHeight, 0.95f)); // southExit
                    CreatePath(path, TravelObjects.Street, straightSpeed, false, waypoints);
                    path.Clear();
                    AddConnection(new(streetOffset, defaultHeight, 0.95f), new(sidewalkOffset, defaultHeight, 0.95f), waypoints); // Connect northExit to sidewalk

                    // Sidewalks
                    AddSidewalkStraight(Directions.East, pedestrianSpeed, waypoints, crosswalk: true);
                    AddSidewalkStraight(Directions.West, pedestrianSpeed, waypoints, crosswalk: true);
                }
                else if (index == ConnectingTile.curve)
                {
                    // north -> east (outer curve)
                    path.Add(new(-streetOffset, defaultHeight, 0.95f));
                    path.Add(new(-streetOffset, defaultHeight, streetOffset));
                    path.Add(new(streetOffset, defaultHeight, -streetOffset));
                    path.Add(new(0.95f, defaultHeight, -streetOffset));
                    CreatePath(path, TravelObjects.Street, straightSpeed, false, waypoints);
                    path.Clear();

                    // Sidewalk
                    path.Add(new(-sidewalkOffset, defaultHeight, 0.95f));//northEntry
                    path.Add(new(-sidewalkOffset, defaultHeight, 0));//beforeCorner
                    path.Add(new(0, defaultHeight, -sidewalkOffset));//afterCorner
                    path.Add(new(0.95f, defaultHeight, -sidewalkOffset)); // eastExit
                    CreatePath(path, TravelObjects.Sidewalk, pedestrianSpeed, true, waypoints);
                    path.Clear();
                    AddSidewalkExit(Directions.West, pedestrianSpeed, waypoints);
                    AddSidewalkExit(Directions.South, pedestrianSpeed, waypoints);

                    // east -> north (inner curve)
                    path.Add(new(0.95f, defaultHeight, streetOffset));
                    path.Add(new(0.5f, defaultHeight, streetOffset));
                    path.Add(new(streetOffset, defaultHeight, 0.5f));
                    path.Add(new(streetOffset, defaultHeight, 0.95f));
                    CreatePath(path, TravelObjects.Street, straightSpeed, false, waypoints);
                    path.Clear();

                    // Sidewalk
                    path.Add(new(0.95f, defaultHeight, sidewalkOffset));
                    path.Add(new(sidewalkOffset, defaultHeight, 0.95f));
                    CreatePath(path, TravelObjects.Sidewalk, pedestrianSpeed, true, waypoints);
                    path.Clear();
                }
                else if (index == ConnectingTile.tJunction)
                {
                    CreateJunction(3, straightSpeed, junctionEntrySpeed, junctionExitSpeed, waypoints);
                    AddSidewalkStraight(Directions.West, pedestrianSpeed, waypoints);
                    AddSidewalkCorner(Directions.North, pedestrianSpeed, waypoints); // north-east corner
                    AddSidewalkCorner(Directions.East, pedestrianSpeed, waypoints); // east-south corner
                }
                else if (index == ConnectingTile.junction)
                {
                    CreateJunction(4, straightSpeed, junctionEntrySpeed, junctionExitSpeed, waypoints);
                    AddSidewalkCorner(Directions.North, pedestrianSpeed, waypoints);
                    AddSidewalkCorner(Directions.East, pedestrianSpeed, waypoints);
                    AddSidewalkCorner(Directions.South, pedestrianSpeed, waypoints);
                    AddSidewalkCorner(Directions.West, pedestrianSpeed, waypoints);
                }
                else
                {
                    Debug.LogError("Unhandled case");
                }
            }

            foreach (KVPair<float3, WaypointData> pair in waypoints)
            {
                float3 pos = pair.Key;
                WaypointData data = pair.Value;

                pos = ToWorldSpace(pos);

                Entity entity = ecb.CreateEntity();
                ecb.AddComponent(entity, LocalTransform.FromPosition(pos));
                ecb.AddComponent(entity, new Waypoint(data.allowedObjects, data.velocity));
                ecb.AddComponent(entity, new Connections(data.entry, data.exit));
                if (data.junctionData != JunctionData.None)
                    ecb.AddComponent(entity, new Junction(data.junctionData));

                // Convert nextWaypoints to world space
                for (int i = 0; i < data.newWaypointData.nextWaypoints.Size; i++)
                {
                    if (math.isnan(data.newWaypointData.nextWaypoints[i].x)) continue;
                    data.newWaypointData.nextWaypoints[i] = ToWorldSpace(data.newWaypointData.nextWaypoints[i]);
                }

                ecb.AddComponent(entity, data.newWaypointData);
            }
            path.Dispose();
            waypoints.Dispose();
        }

        private void CreatePath(NativeList<float3> path, TravelObjects allowedObjects, float velocity, bool connectBackwards, NativeHashMap<float3, WaypointData> waypoints)
        {
            int lastIndex = path.Length - 1;
            for (int i = 0; i < path.Length; i++)
            {
                NewWaypoint newWaypointData = NewWaypoint.Default;
                if (i > 0 && connectBackwards) newWaypointData.AddNext(path[i - 1]); // Connect to previous (if not first and if connectBackwards)
                if (i < lastIndex) newWaypointData.AddNext(path[i + 1]); // Connect to next (if not last)
                bool entry = (i == 0) || (i == lastIndex && connectBackwards);
                bool exit = (i == lastIndex) || (i == 0 && connectBackwards);
                waypoints.Add(path[i], new(allowedObjects, velocity, newWaypointData, entry: entry, exit: exit));
            }
        }

        private void CreateJunction(int dirCount, float straightSpeed, float jEntrySpeed, float jExitSpeed, NativeHashMap<float3, WaypointData> waypoints)
        {
            float3 jExit = new(streetOffset, defaultHeight, 0.5f);

            for (int i = 0; i < dirCount; i++) // Iterate over sides
            {
                Direction dir = (Direction)i;

                // North and south have priority, east and west need to give way (temporary solution)
                JunctionData junctionData = (dir == Directions.North || dir == Directions.South) ? JunctionData.Priority : JunctionData.GiveWay;

                float3 tileEntry = Rotate(new(-streetOffset, defaultHeight, 0.95f), dir);
                float3 junctionEntry = Rotate(new(-streetOffset, defaultHeight, 0.5f), dir);
                float3 junctionExit = Rotate(jExit, dir);
                float3 tileExit = Rotate(new(streetOffset, defaultHeight, 0.95f), dir);

                waypoints.Add(tileEntry, new(TravelObjects.Street, straightSpeed, new(junctionEntry), entry: true));
                waypoints.Add(junctionEntry, new(TravelObjects.Street, jEntrySpeed, NewWaypoint.Default, junctionData));
                waypoints.Add(junctionExit, new(TravelObjects.Street, jExitSpeed, new(tileExit)));
                waypoints.Add(tileExit, new(TravelObjects.Street, straightSpeed, NewWaypoint.Default, exit: true));

                // Add all junction exits (except this dir) to the junction entry
                for (int j = 0; j < dirCount; j++)
                {
                    if (j == i) continue; // Skip self
                    AddConnection(junctionEntry, Rotate(jExit, (Direction)j), waypoints);
                }
            }
        }
        private void AddSidewalkStraight(Direction dir, float pedestrianSpeed, NativeHashMap<float3, WaypointData> waypoints, bool crosswalk = false)
        {
            float3 entry = Rotate(new(0.95f, defaultHeight, sidewalkOffset), dir);
            float3 middle = Rotate(new(0, defaultHeight, sidewalkOffset), dir);
            float3 exit = Rotate(new(-0.95f, defaultHeight, sidewalkOffset), dir);

            NativeList<float3> path = new(Allocator.TempJob) { entry, middle, exit };
            CreatePath(path, TravelObjects.Sidewalk, pedestrianSpeed, true, waypoints);
            path.Dispose();

            AddSidewalkExit(Direction.Rotate(Directions.North, (int)dir), pedestrianSpeed, waypoints);

            AddConnection(exit, Rotate(new(-0.95f, defaultHeight, streetOffset), dir), waypoints); // Connect to road => parking 

            if (crosswalk)
            {
                float3 otherMiddle = Rotate(new(0, defaultHeight, -sidewalkOffset), dir);
                waypoints[middle].newWaypointData.AddNext(otherMiddle);
                AddConnection(middle, otherMiddle, waypoints);
            }
        }

        /// <remarks>dir=north is interpreted as north-east corner</remarks>
        private void AddSidewalkCorner(Direction dir, float pedestrianSpeed, NativeHashMap<float3, WaypointData> waypoints)
        {
            NativeList<float3> path = new(Allocator.TempJob)
            {
                Rotate(new(0.95f, defaultHeight, sidewalkOffset), dir),
                Rotate(new(sidewalkOffset, defaultHeight, sidewalkOffset), dir),
                Rotate(new(sidewalkOffset, defaultHeight, 0.95f), dir)
            };
            CreatePath(path, TravelObjects.Sidewalk, pedestrianSpeed, true, waypoints);
            path.Dispose();
        }

        private void AddSidewalkExit(Direction dir, float pedestrianSpeed, NativeHashMap<float3, WaypointData> waypoints)
        {
            float3 exit = Rotate(new(0, defaultHeight, 0.95f), dir);
            float3 connection = Rotate(new(0, defaultHeight, sidewalkOffset), dir);
            waypoints.Add(exit, new(TravelObjects.Sidewalk, pedestrianSpeed, new(connection), entry: true, exit: true));
            AddConnection(connection, exit, waypoints);
        }

        private void AddConnection(float3 from, float3 to, NativeHashMap<float3, WaypointData> waypoints)
        {
            WaypointData data = waypoints[from];
            data.newWaypointData.AddNext(to);
            waypoints[from] = data;
        }

        private float3 Rotate(float3 pos, Direction dir)
        {
            pos = math.rotate(quaternion.EulerXYZ(0, dir.ToRadians(), 0), pos);
            pos = math.round(pos * 100) / 100; // Round to precision of 0.01
            return pos;
        }
        private float3 ToWorldSpace(float3 pos)
        {
            pos = Rotate(pos, tile.ValueRO.rotation);
            pos += transform.ValueRO.Position;
            return pos;
        }
    }
}