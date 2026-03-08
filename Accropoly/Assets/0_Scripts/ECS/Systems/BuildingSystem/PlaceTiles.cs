using Unity.Entities;
using Components;
using Unity.Mathematics;
using Unity.Transforms;
using Tags;
using Unity.Collections;
using UnityEngine;

namespace Systems
{
    /// <summary>
    /// Replaces tiles with the Replace tag (created by BuildingSystem)
    /// </summary>
    [UpdateInGroup(typeof(CreationSystemGroup))]
    [UpdateBefore(typeof(BuildingSystem))]
    public partial class PlaceTiles : SystemBase
    {
        EntityQuery entitiesToReplaceQuery;
        protected override void OnCreate()
        {
            entitiesToReplaceQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Replace>().Build(this);

            RequireForUpdate<TileToPlaceInfo>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndCreationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            var tileToPlaceInfo = SystemAPI.GetSingleton<TileToPlaceInfo>();
            NativeArray<Entity> entitiesToReplace = entitiesToReplaceQuery.ToEntityArray(Allocator.Temp);

            foreach (Entity entity in entitiesToReplace)
            {
                Tile tile = SystemAPI.GetComponent<Tile>(entity);

                (TileType newTileType, _) = TilePlacingUtility.GetPlacingData(tile.tileType, tileToPlaceInfo.tileType);

                // Set the archetype to the archetype of the newTileType
                var components = TilePlacingUtility.GetComponents(newTileType, tile.pos, tileToPlaceInfo.rotation);

                TilePlacingUtility.UpdateEntity(entity, components);

                // Set the transform rotation according to the rotation of tileToPlace
                RefRW<LocalTransform> transform = SystemAPI.GetComponentRW<LocalTransform>(entity);
                transform.ValueRW.Position = 2 * new float3(tile.pos.x, 0, tile.pos.y);
                transform.ValueRW.Rotation = quaternion.EulerXYZ(0, tileToPlaceInfo.rotation.ToRadians(), 0);
            }
        }
    }
}