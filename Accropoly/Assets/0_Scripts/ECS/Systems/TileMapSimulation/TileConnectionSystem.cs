using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class TileConnectionSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<EntityBufferElement>();
    }
    protected override void OnUpdate()
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        var buffer = SystemAPI.GetBuffer<EntityBufferElement>(SystemAPI.GetSingletonEntity<EntityGridHolder>());

        // ConnectingTile, MapTileComponent & LocalTransform can't be passed as parameters because SystemAPI.GetComponent & SystemAPI.HasComponent are used
        Entities.WithAll<NewTileTag, ConnectingTile>().ForEach((Entity entity) =>
        {
            ConnectingTile connectingTile = SystemAPI.GetComponent<ConnectingTile>(entity);
            MapTileComponent mapTileComponent = SystemAPI.GetComponent<MapTileComponent>(entity);
            LocalTransform transform = SystemAPI.GetComponent<LocalTransform>(entity);

            foreach (Direction direction in Direction.GetDirections())
            {
                if (!TileGridUtility.TryGetTile(mapTileComponent.pos + direction.DirectionVec, buffer, out Entity neighbour)) continue;
                if (SystemAPI.HasComponent<ConnectingTile>(neighbour))
                {
                    var neighbourConnectingTile = SystemAPI.GetComponent<ConnectingTile>(neighbour);

                    // Update self
                    connectingTile.AddDirection(direction);

                    // Update neighbour ConnectingTile
                    neighbourConnectingTile.AddDirection(direction.Flip());
                    ecb.SetComponent(neighbour, neighbourConnectingTile);

                    // Update neighbour material and mesh
                    MaterialsAndMeshesHolder.UpdateAppearence(neighbour, SystemAPI.GetComponent<MapTileComponent>(neighbour).tileType, neighbourConnectingTile);

                    // Update neighbour rotation
                    var neighbourTransform = SystemAPI.GetComponent<LocalTransform>(neighbour);
                    neighbourTransform.Rotation = quaternion.EulerXYZ(0, neighbourConnectingTile.GetRotation().ToRadians(), 0);
                    ecb.SetComponent(neighbour, neighbourTransform);
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
        }).WithoutBurst().Schedule();

        Debug.Log("1");
        if (SystemAPI.HasSingleton<LoadGameTag>())
        {
            Debug.Log("!");
            // ConnectingTile can't be passed as a parameter because SystemAPI.GetComponent & SystemAPI.HasComponent are used
            Entities.WithAll<ConnectingTile>().ForEach((Entity entity, ref MapTileComponent mapTileComponent, ref LocalTransform transform) =>
            {
                Debug.Log("!!!");
                ConnectingTile connectingTile = SystemAPI.GetComponent<ConnectingTile>(entity);
                foreach (Direction direction in Direction.GetDirections())
                {
                    if (!TileGridUtility.TryGetTile(mapTileComponent.pos + direction.DirectionVec, out Entity neighbour)) continue;
                    if (SystemAPI.HasComponent<ConnectingTile>(neighbour))
                    {
                        var neighbourConnectingTile = SystemAPI.GetComponent<ConnectingTile>(neighbour);

                        // Update self
                        connectingTile.AddDirection(direction);
                        Debug.Log(connectingTile);
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