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
        public const float travelSecondsPerSecond = 0.007f; // Slow down travel time. If cars would use the normal timeSpeed, they would be way too fast.
        public const float defaultHeight = 0.8f;

        /// <summary>
        /// Get all directions a car can travel to (from this tile)
        /// </summary>
        public readonly void GetDirections(ref NativeList<Direction> directions)
        {
            Debug.Assert(directions.IsCreated);

            if (connectingTile.IsValid)
            {
                foreach (Direction dir in Direction.GetDirections())
                {
                    if (connectingTile.ValueRO.IsConnected(dir))
                        directions.Add(dir);
                }
            }
            else
                throw new();
        }

        public readonly void GetPoints(EntityCommandBuffer ecb)
        {
            const float straightSpeed = 17;
            const float curveSpeed = 11;
            const float junctionEntrySpeed = 8;
            const float junctionExitSpeed = 11;
            const float pedestrianSpeed = 3;

            // The tile is assumed to face north (other entries/exits follow clockwise)

            // Handle buildings with waypoints (habitats, employers, ...)
            if (!(tile.ValueRO.tileType == TileType.Street || tile.ValueRO.tileType == TileType.CityStreet || tile.ValueRO.tileType == TileType.ForestStreet))
            {
                float3 center = new(0, defaultHeight, 0);
                float3 edge = new(0, defaultHeight, 0.95f);
                AddWaypoint(center, TravelObjects.Sidewalk, pedestrianSpeed, new(edge), ecb);
                AddWaypoint(edge, TravelObjects.Sidewalk, pedestrianSpeed, new(center), ecb, entry: true, exit: true);
                return;
            }

            Debug.Assert(connectingTile.IsValid);

            int index = connectingTile.ValueRO.GetIndex();

            if (index == ConnectingTile.notConnected)
            {

            }
            else if (index == ConnectingTile.deadEnd)
            {
                // Street
                float3 northEntry = new(-streetOffset, defaultHeight, 0.95f);
                float3 centerEntry = new(-streetOffset, defaultHeight, -streetOffset);
                float3 centerExit = new(streetOffset, defaultHeight, -streetOffset);
                float3 northExit = new(streetOffset, defaultHeight, 0.95f);
                AddWaypoint(northEntry, TravelObjects.Street, straightSpeed, new(centerEntry), ecb, entry: true);
                AddWaypoint(centerEntry, TravelObjects.Street, junctionEntrySpeed, new(centerExit), ecb);
                AddWaypoint(centerExit, TravelObjects.Street, junctionEntrySpeed, new(northExit), ecb);
                AddWaypoint(northExit, TravelObjects.Street, straightSpeed, NewWaypoint.Default, ecb, exit: true);

                // Sidewalks
                northEntry = new(-sidewalkOffset, defaultHeight, 0.95f);
                float3 entryMiddle = new(-sidewalkOffset, defaultHeight, 0);
                centerEntry = new(-sidewalkOffset, defaultHeight, -sidewalkOffset);
                float3 centerMiddle = new(0, defaultHeight, -sidewalkOffset);
                centerExit = new(sidewalkOffset, defaultHeight, -sidewalkOffset);
                float3 exitMiddle = new(sidewalkOffset, defaultHeight, 0);
                northExit = new(sidewalkOffset, defaultHeight, 0.95f);
                AddWaypoint(northEntry, TravelObjects.Sidewalk, pedestrianSpeed, new(entryMiddle), ecb, entry: true);
                AddWaypoint(entryMiddle, TravelObjects.Sidewalk, pedestrianSpeed, new(centerEntry, AddSidewalkExit(Directions.West, pedestrianSpeed, ecb)), ecb);
                AddWaypoint(centerEntry, TravelObjects.Sidewalk, pedestrianSpeed, new(centerMiddle), ecb);
                AddWaypoint(centerMiddle, TravelObjects.Sidewalk, pedestrianSpeed, new(centerExit, AddSidewalkExit(Directions.South, pedestrianSpeed, ecb)), ecb);
                AddWaypoint(centerExit, TravelObjects.Sidewalk, pedestrianSpeed, new(exitMiddle), ecb);
                AddWaypoint(exitMiddle, TravelObjects.Sidewalk, pedestrianSpeed, new(northExit, AddSidewalkExit(Directions.East, pedestrianSpeed, ecb)), ecb);
                AddWaypoint(northExit, TravelObjects.Sidewalk, pedestrianSpeed, NewWaypoint.Default, ecb, exit: true);
            }
            else if (index == ConnectingTile.straight)
            {
                // north -> south
                float3 northEntry = new(-streetOffset, defaultHeight, 0.95f);
                float3 southExit = new(-streetOffset, defaultHeight, -0.95f);
                AddWaypoint(northEntry, TravelObjects.Street, straightSpeed, new(southExit), ecb, entry: true);
                AddWaypoint(southExit, TravelObjects.Street, straightSpeed, new(new(-sidewalkOffset, defaultHeight, -0.95f)), ecb, exit: true);

                // south -> north
                float3 southEntry = new(streetOffset, defaultHeight, -0.95f);
                float3 northExit = new(streetOffset, defaultHeight, 0.95f);
                AddWaypoint(southEntry, TravelObjects.Street, straightSpeed, new(northExit), ecb, entry: true);
                AddWaypoint(northExit, TravelObjects.Street, straightSpeed, new(new(sidewalkOffset, defaultHeight, 0.95f)), ecb, exit: true);

                // Sidewalks
                AddSidewalkStraight(Directions.East, pedestrianSpeed, ecb);
                AddSidewalkStraight(Directions.West, pedestrianSpeed, ecb);
            }
            else if (index == ConnectingTile.curve)
            {
                // north -> east (outer curve)
                float3 northEntry = new(-streetOffset, defaultHeight, 0.95f);
                float3 beforeCorner = new(-streetOffset, defaultHeight, streetOffset);
                float3 afterCorner = new(streetOffset, defaultHeight, -streetOffset);
                float3 eastExit = new(0.95f, defaultHeight, -streetOffset);
                AddWaypoint(northEntry, TravelObjects.Street, straightSpeed, new(beforeCorner), ecb, entry: true);
                AddWaypoint(beforeCorner, TravelObjects.Street, curveSpeed, new(afterCorner), ecb);
                AddWaypoint(afterCorner, TravelObjects.Street, curveSpeed, new(eastExit), ecb);
                AddWaypoint(eastExit, TravelObjects.Street, straightSpeed, NewWaypoint.Default, ecb, exit: true);

                // Sidewalk
                northEntry = new(-sidewalkOffset, defaultHeight, 0.95f);
                beforeCorner = new(-sidewalkOffset, defaultHeight, 0);
                afterCorner = new(0, defaultHeight, -sidewalkOffset);
                eastExit = new(0.95f, defaultHeight, -sidewalkOffset);
                AddWaypoint(northEntry, TravelObjects.Sidewalk, pedestrianSpeed, new(beforeCorner), ecb, entry: true);
                AddWaypoint(beforeCorner, TravelObjects.Sidewalk, pedestrianSpeed, new(afterCorner, AddSidewalkExit(Directions.West, pedestrianSpeed, ecb)), ecb);
                AddWaypoint(afterCorner, TravelObjects.Sidewalk, pedestrianSpeed, new(eastExit, AddSidewalkExit(Directions.South, pedestrianSpeed, ecb)), ecb);
                AddWaypoint(eastExit, TravelObjects.Sidewalk, pedestrianSpeed, NewWaypoint.Default, ecb, exit: true);

                // east -> north (inner curve)
                float3 eastEntry = new(0.95f, defaultHeight, streetOffset);
                beforeCorner = new(0.5f, defaultHeight, streetOffset);
                afterCorner = new(streetOffset, defaultHeight, 0.5f);
                float3 northExit = new(streetOffset, defaultHeight, 0.95f);
                AddWaypoint(eastEntry, TravelObjects.Street, straightSpeed, new(beforeCorner), ecb, entry: true);
                AddWaypoint(beforeCorner, TravelObjects.Street, curveSpeed, new(afterCorner), ecb);
                AddWaypoint(afterCorner, TravelObjects.Street, curveSpeed, new(northExit), ecb);
                AddWaypoint(northExit, TravelObjects.Street, straightSpeed, NewWaypoint.Default, ecb, exit: true);

                // Sidewalk
                eastEntry = new(0.95f, defaultHeight, sidewalkOffset);
                northExit = new(sidewalkOffset, defaultHeight, 0.95f);
                AddWaypoint(eastEntry, TravelObjects.Sidewalk, straightSpeed, new(northExit), ecb, entry: true);
                AddWaypoint(northExit, TravelObjects.Sidewalk, straightSpeed, NewWaypoint.Default, ecb, exit: true);
            }
            else if (index == ConnectingTile.tJunction)
            {
                CreateJunction(3, straightSpeed, junctionEntrySpeed, junctionExitSpeed, ecb);
                AddSidewalkStraight(Directions.West, pedestrianSpeed, ecb);
                AddSidewalkCorner(Directions.North, pedestrianSpeed, ecb); // north-east corner
                AddSidewalkCorner(Directions.East, pedestrianSpeed, ecb); // east-south corner
            }
            else if (index == ConnectingTile.junction)
            {
                CreateJunction(4, straightSpeed, junctionEntrySpeed, junctionExitSpeed, ecb);
                AddSidewalkCorner(Directions.North, pedestrianSpeed, ecb);
                AddSidewalkCorner(Directions.East, pedestrianSpeed, ecb);
                AddSidewalkCorner(Directions.South, pedestrianSpeed, ecb);
                AddSidewalkCorner(Directions.West, pedestrianSpeed, ecb);
            }
            else
            {
                Debug.LogError("Unhandled case");
            }
        }

        private void CreateJunction(int dirCount, float straightSpeed, float jEntrySpeed, float jExitSpeed, EntityCommandBuffer ecb)
        {
            for (int i = 0; i < dirCount; i++) // Iterate over sides
            {
                // Calculate waypoint position
                Direction dir = (Direction)i;
                (float3 tileEntry, float3 tileExit) = GetStreetEntryExit(dir);
                (float3 junctionEntry, float3 junctionExit) = GetJunctionEntryExit(dir);

                // North and south have priority, east and west need to give way (temporary solution)
                JunctionData junctionData = (dir == Directions.North || dir == Directions.South) ? JunctionData.Priority : JunctionData.GiveWay;

                // Create tileEntry, tileExit and junctionExit waypoints
                AddWaypoint(tileEntry, TravelObjects.Street, straightSpeed, new(junctionEntry), ecb, entry: true);
                AddWaypoint(tileExit, TravelObjects.Street, straightSpeed, NewWaypoint.Default, ecb, exit: true);
                AddWaypoint(junctionExit, TravelObjects.Street, jExitSpeed, new(tileExit), ecb);

                // Create junctionEntry waypoint (and add all junctionExits as next waypoints)
                NewWaypoint data = NewWaypoint.Default;
                for (int j = 0; j < dirCount; j++)
                {
                    if (j == i) continue; // Skip self
                    (_, float3 exit) = GetJunctionEntryExit((Direction)j);
                    data.AddNext(exit); // Add junction exit to nextWaypoints
                }
                AddWaypoint(junctionEntry, TravelObjects.Street, jEntrySpeed, data, ecb, junctionData: junctionData);
            }
        }
        private (float3, float3) GetStreetEntryExit(Direction dir)
        {
            float3 tileEntry = math.rotate(quaternion.EulerXYZ(0, dir.ToRadians(), 0), new(-streetOffset, defaultHeight, 0.95f));
            float3 tileExit = math.rotate(quaternion.EulerXYZ(0, dir.ToRadians(), 0), new(streetOffset, defaultHeight, 0.95f));
            return (tileEntry, tileExit);
        }
        private (float3, float3) GetJunctionEntryExit(Direction dir)
        {
            float3 junctionEntry = math.rotate(quaternion.EulerXYZ(0, dir.ToRadians(), 0), new(-streetOffset, defaultHeight, 0.5f));
            float3 junctionExit = math.rotate(quaternion.EulerXYZ(0, dir.ToRadians(), 0), new(streetOffset, defaultHeight, 0.5f));
            return (junctionEntry, junctionExit);
        }

        private void AddSidewalkStraight(Direction dir, float pedestrianSpeed, EntityCommandBuffer ecb)
        {
            quaternion rotation = quaternion.EulerXYZ(0, dir.ToRadians(), 0);
            float3 eastEntry = math.rotate(rotation, new(0.95f, defaultHeight, sidewalkOffset));
            float3 middle = math.rotate(rotation, new(0, defaultHeight, sidewalkOffset));
            float3 westExit = math.rotate(rotation, new(-0.95f, defaultHeight, sidewalkOffset));
            float3 streetToConnect = math.rotate(rotation, new(-0.95f, defaultHeight, streetOffset)); // Connect to road -> parking 
            AddWaypoint(eastEntry, TravelObjects.Sidewalk, pedestrianSpeed, new(middle), ecb, entry: true);
            AddWaypoint(middle, TravelObjects.Sidewalk, pedestrianSpeed, new(westExit, AddSidewalkExit(Direction.Rotate(Directions.North, (int)dir), pedestrianSpeed, ecb)), ecb);
            AddWaypoint(westExit, TravelObjects.Sidewalk, pedestrianSpeed, new(streetToConnect), ecb, exit: true);
        }

        /// <remarks>dir=north is interpreted as north-east corner</remarks>
        private void AddSidewalkCorner(Direction dir, float pedestrianSpeed, EntityCommandBuffer ecb)
        {
            quaternion rotation = quaternion.EulerXYZ(0, dir.ToRadians(), 0);
            float3 eastEntry = math.rotate(rotation, new(0.95f, defaultHeight, sidewalkOffset));
            float3 center = math.rotate(rotation, new(sidewalkOffset, defaultHeight, sidewalkOffset));
            float3 northExit = math.rotate(rotation, new(sidewalkOffset, defaultHeight, 0.95f));
            AddWaypoint(eastEntry, TravelObjects.Sidewalk, pedestrianSpeed, new(center), ecb, entry: true);
            AddWaypoint(center, TravelObjects.Sidewalk, pedestrianSpeed, new(northExit), ecb);
            AddWaypoint(northExit, TravelObjects.Sidewalk, pedestrianSpeed, NewWaypoint.Default, ecb, exit: true);
        }

        /// <remarks>Add the return value to the middle sidewalk waypoint on that side</remarks>
        private float3 AddSidewalkExit(Direction dir, float pedestrianSpeed, EntityCommandBuffer ecb)
        {
            float3 exit = math.rotate(quaternion.EulerXYZ(0, dir.ToRadians(), 0), new(0, defaultHeight, 0.95f));
            float3 connection = math.rotate(quaternion.EulerXYZ(0, dir.ToRadians(), 0), new(0, defaultHeight, sidewalkOffset));
            AddWaypoint(exit, TravelObjects.Sidewalk, pedestrianSpeed, new(connection), ecb, entry: true, exit: true);
            return exit;
        }

        private void AddWaypoint(float3 pos, TravelObjects TravelObjects, float velocity, NewWaypoint newWaypointData, EntityCommandBuffer ecb,
            JunctionData junctionData = JunctionData.None, bool entry = false, bool exit = false)
        {
            pos = ToWorldSpace(pos);

            Entity entity = ecb.CreateEntity();
            ecb.AddComponent(entity, LocalTransform.FromPosition(pos));
            ecb.AddComponent(entity, new Waypoint(TravelObjects, velocity));
            ecb.AddComponent(entity, new Connections(entry, exit));
            if (junctionData != JunctionData.None)
                ecb.AddComponent(entity, new Junction(junctionData));

            // Convert nextWaypoints to world space
            for (int i = 0; i < newWaypointData.nextWaypoints.Size; i++)
            {
                if (math.isnan(newWaypointData.nextWaypoints[i].x)) continue;
                newWaypointData.nextWaypoints[i] = ToWorldSpace(newWaypointData.nextWaypoints[i]);
            }

            ecb.AddComponent(entity, newWaypointData);
        }
        private float3 ToWorldSpace(float3 pos)
        {
            pos = math.rotate(quaternion.EulerXYZ(0, tile.ValueRO.rotation.ToRadians(), 0), pos);
            pos += transform.ValueRO.Position;
            pos = math.round(pos * 100) / 100; // Round to precision of 0.01
            return pos;
        }
    }
}