using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Components;
using Tags;

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

            // Connect new tiles with the ConnectingTile component
            // ConnectingTile, MapTileComponent & LocalTransform can't be passed as parameters because SystemAPI.GetComponent & SystemAPI.HasComponent are used
            Entities.WithAll<NewTile, ConnectingTile>().ForEach((Entity entity) =>
            {
                ConnectingTile connectingTile = SystemAPI.GetComponent<ConnectingTile>(entity);
                Tile mapTileComponent = SystemAPI.GetComponent<Tile>(entity);
                LocalTransform transform = SystemAPI.GetComponent<LocalTransform>(entity);

                foreach (Direction direction in Direction.GetDirections())
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
                            MaterialsAndMeshesHolder.UpdateAppearence(neighbour, SystemAPI.GetComponent<Tile>(neighbour).tileType, neighbourConnectingTile);
                            var neighbourTransform = SystemAPI.GetComponent<LocalTransform>(neighbour);
                            neighbourTransform.Rotation = quaternion.EulerXYZ(0, neighbourConnectingTile.GetRotation().ToRadians(), 0);
                            ecb.SetComponent(neighbour, neighbourTransform);
                        }
                    }
                }

                // Update ConnectingTile
                ecb.SetComponent(entity, connectingTile);

                // Update material and mesh
                MaterialsAndMeshesHolder.UpdateAppearence(entity, mapTileComponent.tileType, connectingTile);

                // Update rotation
                Direction rotation = connectingTile.GetRotation();
                transform.Rotation = quaternion.EulerXYZ(0, rotation.ToRadians(), 0);
                mapTileComponent.rotation = rotation;
                ecb.SetComponent(entity, transform);
                ecb.SetComponent(entity, mapTileComponent);
            }).Schedule();

            // Disconnect new tiles with the ConnectingTile component
            Entities.WithAll<NewTile>().WithNone<ConnectingTile>().ForEach((Entity entity) =>
            {
                var mapTileComponent = SystemAPI.GetComponent<Tile>(entity);

                foreach (Direction direction in Direction.GetDirections())
                {
                    if (!TileGridUtility.TryGetTile(mapTileComponent.pos + direction.DirectionVec, entityGrid, out Entity neighbour)) continue;
                    if (SystemAPI.HasComponent<ConnectingTile>(neighbour))
                    {
                        var neighbourConnectingTile = SystemAPI.GetComponent<ConnectingTile>(neighbour);

                        // Update neighbour ConnectingTile
                        neighbourConnectingTile.RemoveDirection(direction.Flip());
                        ecb.SetComponent(neighbour, neighbourConnectingTile);

                        // Update neighbour material and mesh
                        MaterialsAndMeshesHolder.UpdateAppearence(neighbour, SystemAPI.GetComponent<Tile>(neighbour).tileType, neighbourConnectingTile);

                        // Update neighbour rotation
                        var neighbourTransform = SystemAPI.GetComponent<LocalTransform>(neighbour);
                        neighbourTransform.Rotation = quaternion.EulerXYZ(0, neighbourConnectingTile.GetRotation().ToRadians(), 0);
                        ecb.SetComponent(neighbour, neighbourTransform);
                    }
                }
            }).Schedule();

            if (SystemAPI.HasSingleton<LoadGame>())
            {
                // ConnectingTile can't be passed as a parameter because SystemAPI.GetComponent & SystemAPI.HasComponent are used
                Entities.WithAll<ConnectingTile>().ForEach((Entity entity, ref Tile mapTileComponent, ref LocalTransform transform) =>
                {
                    ConnectingTile connectingTile = SystemAPI.GetComponent<ConnectingTile>(entity);
                    foreach (Direction direction in Direction.GetDirections())
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

                    // Update material and mesh
                    MaterialsAndMeshesHolder.UpdateAppearence(entity, mapTileComponent.tileType, connectingTile);

                    // Update rotation
                    Direction rotation = connectingTile.GetRotation();
                    transform.Rotation = quaternion.EulerXYZ(0, rotation.ToRadians(), 0);
                    mapTileComponent.rotation = rotation;
                }).Schedule();
            }
        }
    }
}