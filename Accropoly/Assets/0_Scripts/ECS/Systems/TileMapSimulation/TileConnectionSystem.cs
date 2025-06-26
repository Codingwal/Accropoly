using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Components;
using Tags;
using Unity.Collections;

namespace Systems
{
    /// <summary>
    /// Handle connections between ConnectingTile tiles (streets, rivers, ...)
    /// and update their appearence accordingly
    /// </summary>
    public partial class TileConnectionSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<EntityBufferElement>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            var entityGrid = TileGridUtility.GetEntityGrid();

            NativeArray<Direction> directions = new(4, Allocator.TempJob);
            Direction.GetDirections(ref directions);

            // Connect new tiles with the ConnectingTile component
            // ConnectingTile, MapTileComponent & LocalTransform can't be passed as parameters because SystemAPI.GetComponent & SystemAPI.HasComponent are used
            Entities.WithAll<NewTile, ConnectingTile>().ForEach((Entity entity) =>
            {
                ConnectingTile connectingTile = SystemAPI.GetComponent<ConnectingTile>(entity);
                Tile mapTileComponent = SystemAPI.GetComponent<Tile>(entity);
                LocalTransform transform = SystemAPI.GetComponent<LocalTransform>(entity);

                foreach (Direction direction in directions)
                {
                    if (!TileGridUtility.TryGetTile(mapTileComponent.pos + direction.DirectionVec, entityGrid, out Entity neighbour)) continue;
                    if (SystemAPI.HasComponent<ConnectingTile>(neighbour))
                    {
                        var neighbourConnectingTile = SystemAPI.GetComponent<ConnectingTile>(neighbour);
                        if (neighbourConnectingTile.group == connectingTile.group)
                        {
                            connectingTile.AddDirection(direction);
                            neighbourConnectingTile.AddDirection(direction.Flip());
                        }
                        else neighbourConnectingTile.RemoveDirection(direction.Flip());

                        // Update neighbour if they don't get updated this frame anyway
                        if (!SystemAPI.HasComponent<NewTile>(neighbour))
                        {
                            SystemAPI.SetComponent(neighbour, neighbourConnectingTile); // Changes must be applied directly as they might be read in the same job

                            var neighbourTransform = SystemAPI.GetComponent<LocalTransform>(neighbour);
                            neighbourTransform.Rotation = quaternion.EulerXYZ(0, neighbourConnectingTile.GetRotation().ToRadians(), 0);
                            SystemAPI.SetComponent(neighbour, neighbourTransform);

                            var neighbourTile = SystemAPI.GetComponent<Tile>(neighbour);
                            neighbourTile.rotation = neighbourConnectingTile.GetRotation();
                            SystemAPI.SetComponent(neighbour, neighbourTile); // Changes must be applied directly because the data might be updated in the same frame again
                        }
                    }
                }

                // Update ConnectingTile
                ecb.SetComponent(entity, connectingTile);

                // Update rotation
                Direction rotation = connectingTile.GetRotation();
                transform.Rotation = quaternion.EulerXYZ(0, rotation.ToRadians(), 0);
                mapTileComponent.rotation = rotation;
                SystemAPI.SetComponent(entity, transform);
                SystemAPI.SetComponent(entity, mapTileComponent); // Changes must be applied directly because the data might be updated in the same frame again
            }).Schedule();

            // Disconnect new tiles without the ConnectingTile component
            Entities.WithAll<NewTile>().WithNone<ConnectingTile>().ForEach((Entity entity) =>
            {
                var mapTileComponent = SystemAPI.GetComponent<Tile>(entity);

                foreach (Direction direction in directions)
                {
                    if (!TileGridUtility.TryGetTile(mapTileComponent.pos + direction.DirectionVec, entityGrid, out Entity neighbour)) continue;
                    if (SystemAPI.HasComponent<ConnectingTile>(neighbour))
                    {
                        var neighbourConnectingTile = SystemAPI.GetComponent<ConnectingTile>(neighbour);

                        // Update neighbour ConnectingTile
                        neighbourConnectingTile.RemoveDirection(direction.Flip());
                        ecb.SetComponent(neighbour, neighbourConnectingTile);

                        var neighbourTransform = SystemAPI.GetComponent<LocalTransform>(neighbour);
                        neighbourTransform.Rotation = quaternion.EulerXYZ(0, neighbourConnectingTile.GetRotation().ToRadians(), 0);
                        SystemAPI.SetComponent(neighbour, neighbourTransform);

                        var neighbourTile = SystemAPI.GetComponent<Tile>(neighbour);
                        neighbourTile.rotation = neighbourConnectingTile.GetRotation();
                        SystemAPI.SetComponent(neighbour, neighbourTile); // Changes must be applied directly because the data might be updated in the same frame again
                    }
                }
            }).Schedule();

            if (SystemAPI.HasSingleton<LoadGame>())
            {
                // ConnectingTile can't be passed as a parameter because SystemAPI.GetComponent & SystemAPI.HasComponent are used
                Entities.WithAll<ConnectingTile>().ForEach((Entity entity, ref Tile mapTileComponent, ref LocalTransform transform) =>
                {
                    ConnectingTile connectingTile = SystemAPI.GetComponent<ConnectingTile>(entity);
                    foreach (Direction direction in directions)
                    {
                        if (!TileGridUtility.TryGetTile(mapTileComponent.pos + direction.DirectionVec, entityGrid, out Entity neighbour)) continue;
                        if (SystemAPI.HasComponent<ConnectingTile>(neighbour))
                        {
                            var neighbourConnectingTile = SystemAPI.GetComponent<ConnectingTile>(neighbour);
                            if (neighbourConnectingTile.group != connectingTile.group) continue;

                            // Update self
                            connectingTile.AddDirection(direction);
                        }
                    }
                    // Update ConnectingTile
                    ecb.SetComponent(entity, connectingTile);

                    // Update rotation
                    Direction rotation = connectingTile.GetRotation();
                    transform.Rotation = quaternion.EulerXYZ(0, rotation.ToRadians(), 0);
                    mapTileComponent.rotation = rotation;
                }).Schedule();
            }

            directions.Dispose(Dependency);
        }
    }
}