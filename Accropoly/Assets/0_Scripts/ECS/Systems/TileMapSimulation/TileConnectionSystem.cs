using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Components;
using Tags;
using Unity.Collections;
using UnityEngine;
using Unity.Burst;

namespace Systems
{
    /// <summary>
    /// Handle connections between ConnectingTile tiles (streets, rivers, ...)
    /// </summary>
    public partial class TileConnectionSystem : SystemBase
    {
        EntityQuery connectingTiles;
        EntityQuery newConnectingTiles;
        EntityQuery newNotConnectingTiles;
        protected override void OnCreate()
        {
            RequireForUpdate<EntityBufferElement>();

            connectingTiles = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ConnectingTile>()
                .Build(this);

            newConnectingTiles = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<NewTile, ConnectingTile>()
                .Build(this);

            newNotConnectingTiles = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<NewTile>()
                .WithNone<ConnectingTile>()
                .Build(this);

        }
        protected override void OnUpdate()
        {
            var entityGrid = TileGridUtility.GetEntityGrid();

            if (SystemAPI.HasSingleton<LoadGame>())
            {
                new ConnectTilesJob
                {
                    entityGrid = entityGrid,
                    connectingTileLookup = SystemAPI.GetComponentLookup<ConnectingTile>(),
                    tileLookup = SystemAPI.GetComponentLookup<Tile>(),
                    transformLookup = SystemAPI.GetComponentLookup<LocalTransform>()
                }.Schedule(connectingTiles);
                return;
            }

            new ConnectTilesJob
            {
                entityGrid = entityGrid,
                connectingTileLookup = SystemAPI.GetComponentLookup<ConnectingTile>(),
                tileLookup = SystemAPI.GetComponentLookup<Tile>(),
                transformLookup = SystemAPI.GetComponentLookup<LocalTransform>()
            }.Schedule(newConnectingTiles);

            new DisconnectTilesJob
            {
                entityGrid = entityGrid,
                connectingTileLookup = SystemAPI.GetComponentLookup<ConnectingTile>(),
                tileLookup = SystemAPI.GetComponentLookup<Tile>(),
                transformLookup = SystemAPI.GetComponentLookup<LocalTransform>()
            }.Schedule(newNotConnectingTiles);
        }

        // TODO: BurstCompile (Direction.GetDirections must become unmanaged first)
        private partial struct ConnectTilesJob : IJobEntity
        {
            public DynamicBuffer<EntityBufferElement> entityGrid;
            public ComponentLookup<ConnectingTile> connectingTileLookup;
            public ComponentLookup<Tile> tileLookup;
            public ComponentLookup<LocalTransform> transformLookup;
            public void Execute(Entity entity)
            {
                RefRW<ConnectingTile> connectingTile = connectingTileLookup.GetRefRW(entity);
                RefRW<Tile> tile = tileLookup.GetRefRW(entity);

                foreach (Direction direction in Direction.GetDirections())
                {
                    if (!TileGridUtility.TryGetTile(tile.ValueRO.pos + direction.DirectionVec, entityGrid, out Entity neighbour)) continue;
                    if (!connectingTileLookup.HasComponent(neighbour)) continue;

                    var neighbourConnectingTile = connectingTileLookup.GetRefRW(neighbour);
                    if (neighbourConnectingTile.ValueRO.group == connectingTile.ValueRO.group)
                    {
                        connectingTile.ValueRW.AddDirection(direction);
                        neighbourConnectingTile.ValueRW.AddDirection(direction.Flip());
                    }
                    else neighbourConnectingTile.ValueRW.RemoveDirection(direction.Flip());

                    // Update neighbour
                    Direction newNeighbourRotation = neighbourConnectingTile.ValueRO.GetRotation();
                    transformLookup.GetRefRW(neighbour).ValueRW.Rotation = quaternion.EulerXYZ(0, newNeighbourRotation.ToRadians(), 0);
                    tileLookup.GetRefRW(neighbour).ValueRW.rotation = newNeighbourRotation;
                }

                Direction newRotation = connectingTile.ValueRO.GetRotation();
                tile.ValueRW.rotation = newRotation;
                transformLookup.GetRefRW(entity).ValueRW.Rotation = quaternion.EulerXYZ(0, newRotation.ToRadians(), 0);
            }
        }

        // TODO: BurstCompile (Direction.GetDirections must become unmanaged first)
        private partial struct DisconnectTilesJob : IJobEntity
        {
            public DynamicBuffer<EntityBufferElement> entityGrid;
            public ComponentLookup<ConnectingTile> connectingTileLookup;
            public ComponentLookup<Tile> tileLookup;
            public ComponentLookup<LocalTransform> transformLookup;
            public void Execute(Entity entity)
            {
                Tile tile = tileLookup.GetRefRO(entity).ValueRO;

                foreach (Direction direction in Direction.GetDirections())
                {
                    if (!TileGridUtility.TryGetTile(tile.pos + direction.DirectionVec, entityGrid, out Entity neighbour)) continue;
                    if (!connectingTileLookup.HasComponent(neighbour)) continue;

                    var neighbourConnectingTile = connectingTileLookup.GetRefRW(neighbour);

                    // Update neighbourr connecting tile
                    neighbourConnectingTile.ValueRW.RemoveDirection(direction.Flip());

                    // Update neighbour rotation
                    Direction newNeighbourRotation = neighbourConnectingTile.ValueRO.GetRotation();
                    transformLookup.GetRefRW(neighbour).ValueRW.Rotation = quaternion.EulerXYZ(0, newNeighbourRotation.ToRadians(), 0);
                    tileLookup.GetRefRW(neighbour).ValueRW.rotation = newNeighbourRotation;
                }
            }
        }
    }
}