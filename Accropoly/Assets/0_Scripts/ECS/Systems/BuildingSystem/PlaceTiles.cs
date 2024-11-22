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

            var gameInfo = SystemAPI.GetSingleton<GameInfo>();

            var tileToPlace = SystemAPI.GetSingleton<TileToPlace>();
            TileType newTileType = tileToPlace.tileType;
            float price = SystemAPI.ManagedAPI.GetSingleton<ConfigComponents.TilePrices>().prices[newTileType];

            Entities.WithAll<Replace>().ForEach((Entity entity, in Tile tile) =>
            {
                // If the tile can be bought, buy it, else, abort
                if (price > gameInfo.balance) // If the tile can't be bought, abort
                {
                    ecb.RemoveComponent<Replace>(entity);
                    return;
                }
                gameInfo.balance -= price; // Buy the tile

                var transform = SystemAPI.GetComponent<LocalTransform>(entity);

                // Set the archetype to the archetype of the newTileType
                var components = TilePlacingUtility.GetComponents(newTileType, tile.pos, tileToPlace.rotation);

                TilePlacingUtility.UpdateEntity(entity, components, ecb);

                // Set the transform rotation according to the rotation of tileToPlace
                transform.Rotation = quaternion.EulerXYZ(0, tileToPlace.rotation.ToRadians(), 0);
                ecb.SetComponent(entity, transform);

                // Set mesh & material according to the new tileType
                MaterialsAndMeshesHolder.UpdateMeshAndMaterial(entity, newTileType);
            }).WithStructuralChanges().Run();

            // Save the updated balance
            ecb.SetComponent(SystemAPI.GetSingletonEntity<GameInfo>(), gameInfo);
        }
    }
}