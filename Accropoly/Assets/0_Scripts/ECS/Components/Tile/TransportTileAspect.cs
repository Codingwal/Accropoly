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

        private const float offsetFromCenter = 0.25f;
        public const float travelSecondsPerSecond = 0.007f; // Slow down travel time. If cars would use the normal timeSpeed, they would be way too fast.
        public const float defaultVerticalOffset = 0.8f;

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
            Debug.Assert(tile.ValueRO.tileType == TileType.Street || tile.ValueRO.tileType == TileType.CityStreet || tile.ValueRO.tileType == TileType.ForestStreet);
            Debug.Assert(connectingTile.IsValid);

            int index = connectingTile.ValueRO.GetIndex();

            // The tile is assumed to face north

            if (index == ConnectingTile.notConnected)
            {
                Debug.LogWarning("!!!");
                return;
            }
            if (index == ConnectingTile.deadEnd)
            {
                float3 northEntry = new(-offsetFromCenter, defaultVerticalOffset, 0.95f);
                float3 centerEntry = new(-offsetFromCenter, defaultVerticalOffset, 0);
                float3 centerExit = new(offsetFromCenter, defaultVerticalOffset, 0);
                float3 northExit = new(offsetFromCenter, defaultVerticalOffset, 0.95f);

                AddWaypoint(northEntry, 15, new(centerEntry), ecb);
                AddWaypoint(centerEntry, 6, new(centerExit), ecb);
                AddWaypoint(centerExit, 6, new(northExit), ecb);
                AddWaypoint(northExit, 15, NewWaypoint.Default, ecb, exit: true);

                return;
            }
            if (index == ConnectingTile.straight)
            {
                // north -> south
                float3 northEntry = new(-offsetFromCenter, defaultVerticalOffset, 0.95f);
                float3 southExit = new(-offsetFromCenter, defaultVerticalOffset, -0.95f);
                AddWaypoint(northEntry, 17, new(southExit), ecb);
                AddWaypoint(southExit, 17, NewWaypoint.Default, ecb, exit: true);

                // south -> north
                float3 southEntry = new(offsetFromCenter, defaultVerticalOffset, -0.95f);
                float3 northExit = new(offsetFromCenter, defaultVerticalOffset, 0.95f);
                AddWaypoint(southEntry, 17, new(northExit), ecb);
                AddWaypoint(northExit, 17, NewWaypoint.Default, ecb, exit: true);

                return;
            }
            if (index == ConnectingTile.curve)
            {
                // north -> east (outer curve)
                float3 northEntry = new(-offsetFromCenter, defaultVerticalOffset, 0.95f);
                float3 beforeCorner = new(-offsetFromCenter, defaultVerticalOffset, offsetFromCenter);
                float3 afterCorner = new(offsetFromCenter, defaultVerticalOffset, -offsetFromCenter);
                float3 eastExit = new(0.95f, defaultVerticalOffset, -offsetFromCenter);
                AddWaypoint(northEntry, 17, new(beforeCorner), ecb);
                AddWaypoint(beforeCorner, 11, new(afterCorner), ecb);
                AddWaypoint(afterCorner, 11, new(eastExit), ecb);
                AddWaypoint(eastExit, 17, NewWaypoint.Default, ecb, exit: true);

                // east -> north (inner curve)
                float3 eastEntry = new(0.95f, defaultVerticalOffset, offsetFromCenter);
                beforeCorner = new(0.5f, defaultVerticalOffset, offsetFromCenter);
                afterCorner = new(offsetFromCenter, defaultVerticalOffset, 0.5f);
                float3 northExit = new(offsetFromCenter, defaultVerticalOffset, 0.95f);
                AddWaypoint(eastEntry, 17, new(beforeCorner), ecb);
                AddWaypoint(beforeCorner, 11, new(afterCorner), ecb);
                AddWaypoint(afterCorner, 11, new(northExit), ecb);
                AddWaypoint(northExit, 17, NewWaypoint.Default, ecb, exit: true);

                return;
            }
            if (index == ConnectingTile.tJunction)
            {
                CreateJunction(3, ecb);
                return;
            }
            if (index == ConnectingTile.junction)
            {
                CreateJunction(4, ecb);
                return;
            }

            Debug.LogError("Unhandled case");
        }
        private void CreateJunction(int dirCount, EntityCommandBuffer ecb)
        {
            for (int i = 0; i < dirCount; i++) // Iterate over sides
            {
                // Calculate waypoint position
                Direction dir = (Direction)i;
                (float3 tileEntry, float3 tileExit) = GetTileEntryExit(dir);
                (float3 junctionEntry, float3 junctionExit) = GetJunctionEntryExit(dir);

                // North and south have priority, east and west need to give way (temporary solution)
                JunctionData junctionData = (dir == Directions.North || dir == Directions.South) ? JunctionData.Priority : JunctionData.GiveWay;

                // Create tileEntry, tileExit and junctionExit waypoints
                AddWaypoint(tileEntry, 17, new(junctionEntry), ecb);
                AddWaypoint(tileExit, 17, NewWaypoint.Default, ecb, exit: true);
                AddWaypoint(junctionExit, 11, new(tileExit), ecb);

                // Create junctionEntry waypoint (and add all junctionExits as next waypoints)
                NewWaypoint data = NewWaypoint.Default;
                for (int j = 0; j < dirCount; j++)
                {
                    if (j == i) continue; // Skip self
                    (_, float3 exit) = GetJunctionEntryExit((Direction)j);
                    data.AddNext(exit); // Add junction exit to nextWaypoints
                }
                AddWaypoint(junctionEntry, 8, data, ecb, junctionData: junctionData);
            }
        }
        private (float3, float3) GetTileEntryExit(Direction dir)
        {
            float3 tileEntry = math.rotate(quaternion.EulerXYZ(0, dir.ToRadians(), 0), new(-offsetFromCenter, defaultVerticalOffset, 0.95f));
            float3 tileExit = math.rotate(quaternion.EulerXYZ(0, dir.ToRadians(), 0), new(offsetFromCenter, defaultVerticalOffset, 0.95f));
            return (tileEntry, tileExit);
        }
        private (float3, float3) GetJunctionEntryExit(Direction dir)
        {
            float3 junctionEntry = math.rotate(quaternion.EulerXYZ(0, dir.ToRadians(), 0), new(-offsetFromCenter, defaultVerticalOffset, 0.5f));
            float3 junctionExit = math.rotate(quaternion.EulerXYZ(0, dir.ToRadians(), 0), new(offsetFromCenter, defaultVerticalOffset, 0.5f));
            return (junctionEntry, junctionExit);
        }
        private void AddWaypoint(float3 pos, float velocity, NewWaypoint newWaypointData, EntityCommandBuffer ecb,
            JunctionData junctionData = JunctionData.None, bool exit = false)
        {
            pos = ToWorldSpace(pos);

            Entity entity = ecb.CreateEntity();
            ecb.AddComponent(entity, LocalTransform.FromPosition(pos));
            ecb.AddComponent(entity, new Waypoint(velocity));
            ecb.AddComponent(entity, new Connections(exit));
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