using Unity.Entities;
using Components;
using Unity.Mathematics;
using Unity.Transforms;
using Tags;

namespace Systems
{
    /// <summary>
    /// Replaces tiles with the Replace tag (created by BuildingSystem)
    /// </summary>
    [UpdateInGroup(typeof(CreationSystemGroup))]
    [UpdateBefore(typeof(BuildingSystem))]
    public partial class PlaceTiles : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<TileToPlaceInfo>();
            RequireForUpdate<Replace>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndCreationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            var tileToPlaceInfo = SystemAPI.GetSingleton<TileToPlaceInfo>();

            Entities.WithAll<Replace>().ForEach((Entity entity, ref LocalTransform transform, in Tile tile) =>
            {
                (TileType newTileType, _) = TilePlacingUtility.GetPlacingData(tile.tileType, tileToPlaceInfo.tileType);

                // Set the archetype to the archetype of the newTileType
                var components = TilePlacingUtility.GetComponents(newTileType, tile.pos, tileToPlaceInfo.rotation);

                TilePlacingUtility.UpdateEntity(entity, components, ecb);

                // Set the transform rotation according to the rotation of tileToPlace
                transform.Rotation = quaternion.EulerXYZ(0, tileToPlaceInfo.rotation.ToRadians(), 0);

                // Set mesh & material according to the new tileType
                MaterialsAndMeshesHolder.UpdateMeshAndMaterial(entity, newTileType);
            }).WithStructuralChanges().Run();
        }
    }
}