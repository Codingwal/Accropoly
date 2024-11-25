using Unity.Entities;
using UnityEngine;
using Components;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Tags;

namespace Systems
{
    [UpdateInGroup(typeof(CreationSystemGroup))]
    [UpdateBefore(typeof(BuildingSystem))]
    public partial class PlaceTiles : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<TileToPlace>();
            RequireForUpdate<Replace>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndCreationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            var tileToPlaceInfo = SystemAPI.GetSingleton<TileToPlaceInfo>();
            TileType newTileType = tileToPlaceInfo.tileType;

            Entities.WithAll<Replace>().ForEach((Entity entity, in Tile tile) =>
            {
                var transform = SystemAPI.GetComponent<LocalTransform>(entity);

                // Set the archetype to the archetype of the newTileType
                var components = TilePlacingUtility.GetComponents(newTileType, tile.pos, tileToPlaceInfo.rotation);

                TilePlacingUtility.UpdateEntity(entity, components, ecb);

                // Set the transform rotation according to the rotation of tileToPlace
                transform.Rotation = quaternion.EulerXYZ(0, tileToPlaceInfo.rotation.ToRadians(), 0);
                ecb.SetComponent(entity, transform);

                // Set mesh & material according to the new tileType
                MaterialsAndMeshesHolder.UpdateMeshAndMaterial(entity, newTileType);
            }).WithStructuralChanges().Run();
        }
    }
}