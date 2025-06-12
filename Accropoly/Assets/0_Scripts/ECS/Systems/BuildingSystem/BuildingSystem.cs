using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Components;
using PlacementAction = Components.PlacementInputData.Action;
using Tags;
using ConfigComponents;

namespace Systems
{
    /// <summary>
    /// Handle tile placing (using the current mouse position calculated by UpdatePosition)
    /// Does not directly place the tiles. 
    /// Instead, they are marked with the Replace tag and the actual replacing is handled by PlaceTiles
    /// </summary>
    [UpdateInGroup(typeof(CreationSystemGroup))]
    public partial class BuildingSystem : SystemBase
    {
        private EntityQuery placementInputDataQuery;
        private static EntityQuery tileToPlaceQuery;
        protected override void OnCreate()
        {
            RequireForUpdate<RunGame>();
            RequireForUpdate<TileToPlaceInfo>(); // Update only if there is a PlacementProcess running (The process is started by the menu)

            placementInputDataQuery = GetEntityQuery(typeof(PlacementInputData));
            tileToPlaceQuery = GetEntityQuery(typeof(TileToPlace));
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndCreationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            // Only execute if there is placementInput (can't use singleton functions bc of IEnableableComponent)
            if (placementInputDataQuery.IsEmpty)
                return;

            // The PlacementInputData is attached to the same entity as the singleton InputData
            var placementInputData = EntityManager.GetComponentData<PlacementInputData>(SystemAPI.GetSingletonEntity<InputData>());
            var tileToPlaceInfo = SystemAPI.GetSingleton<TileToPlaceInfo>();

            if (placementInputData.placementProcessRunning)
            {
                // Place TileToPlace at current pos, if there isn't one already
                var tileToPlaceInfoEntity = SystemAPI.GetSingletonEntity<TileToPlaceInfo>();
                float2 pos = SystemAPI.GetComponent<LocalTransform>(tileToPlaceInfoEntity).Position.xz;
                bool alreadyExists = false;
                foreach (var transform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<TileToPlace>())
                {
                    if (transform.ValueRO.Position.xz.Equals(pos))
                    {
                        alreadyExists = true;
                        break;
                    }
                }
                if (!alreadyExists)
                {
                    // Get new tile type
                    Entity oldTile = TileGridUtility.GetTile((int2)pos / 2);
                    TileType oldTileType = SystemAPI.GetComponent<Tile>(oldTile).tileType;
                    (TileType tileType, _) = TilePlacingUtility.GetPlacingData(oldTileType, tileToPlaceInfo.tileType);

                    // Tile cannot be placed here
                    if (tileType == TileType.None)
                        return;

                    var prefab = SystemAPI.GetSingleton<PrefabEntity>();
                    Entity entity = EntityManager.Instantiate(prefab); // Inefficient but who cares
                    ecb.SetComponent(entity, LocalTransform.FromPositionRotation(new(pos.x, 0.3f, pos.y), quaternion.Euler(new(0, tileToPlaceInfo.rotation.ToRadians(), 0))));
                    ecb.AddComponent<TileToPlace>(entity);
                    MaterialsAndMeshesHolder.UpdateMeshAndMaterial(entity, tileType);
                }
                return;
            }
            else if (placementInputData.action == PlacementAction.Place)
            {
                var gameInfo = SystemAPI.GetSingleton<GameInfo>();

                Entities.WithAll<TileToPlace>().ForEach((in LocalTransform transform) =>
                {
                    int2 pos = (int2)transform.Position.xz / 2;
                    Entity tile = TileGridUtility.GetTile(pos);

                    // Get tile placing cost
                    TileType oldTileType = SystemAPI.GetComponent<Tile>(tile).tileType;
                    (_, int cost) = TilePlacingUtility.GetPlacingData(oldTileType, tileToPlaceInfo.tileType);

                    if (cost <= gameInfo.balance) // If the tile can be bought
                    {
                        gameInfo.balance -= cost; // Buy the tile
                        ecb.AddComponent<Replace>(tile); // Mark tile for placement
                    }
                }).Run();
                ecb.SetComponent(SystemAPI.GetSingletonEntity<GameInfo>(), gameInfo); // Update the balance

                // Delete all TileToPlace entities (as they have been placed now)
                ecb.DestroyEntity(tileToPlaceQuery, EntityQueryCaptureMode.AtPlayback);

                return;
            }
            else if (placementInputData.action == PlacementAction.Rotate)
            {
                tileToPlaceInfo.rotation = tileToPlaceInfo.rotation.Rotate(1);

                // Update transform & TileToPlaceInfo
                Entity tileToPlaceInfoEntity = SystemAPI.GetSingletonEntity<TileToPlaceInfo>();
                var tileToPlaceInfoTransform = SystemAPI.GetComponent<LocalTransform>(tileToPlaceInfoEntity);
                tileToPlaceInfoTransform.Rotation = quaternion.Euler(new(0, tileToPlaceInfo.rotation.ToRadians(), 0));
                ecb.SetComponent(tileToPlaceInfoEntity, tileToPlaceInfoTransform);
                ecb.SetComponent(tileToPlaceInfoEntity, tileToPlaceInfo);

                // Update all TileToPlace entities
                Entities.WithAll<TileToPlace>().ForEach((ref LocalTransform transform) =>
                {
                    transform.Rotation = quaternion.Euler(new(0, tileToPlaceInfo.rotation.ToRadians(), 0));
                }).Schedule();

                return;
            }
            else if (placementInputData.action == PlacementAction.Cancel)
            {
                ecb.DestroyEntity(tileToPlaceQuery, EntityQueryCaptureMode.AtPlayback);
                ecb.DestroyEntity(SystemAPI.GetSingletonEntity<TileToPlaceInfo>());
                return;
            }
        }
        public static void StartPlacementProcess(TileType tileType)
        {
            EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var tileToPlaceInfoQuery = em.CreateEntityQuery(typeof(TileToPlaceInfo));

            // If there already is a placement process running, stop it
            if (!tileToPlaceInfoQuery.IsEmpty)
            {
                em.DestroyEntity(tileToPlaceQuery); // Delete all TileToPlace entities
                em.DestroyEntity(tileToPlaceInfoQuery); // Delete TileToPlaceInfo singleton (will be recreated afterwards (with the new TileType))
            }

            // Create a tileToPlaceInfo singleton entity and set mesh & material
            var prefab = em.CreateEntityQuery(typeof(PrefabEntity)).GetSingleton<PrefabEntity>();
            Entity tileToPlaceInfoEntity = em.Instantiate(prefab);
            em.AddComponentData(tileToPlaceInfoEntity, new TileToPlaceInfo { tileType = tileType });
            MaterialsAndMeshesHolder.UpdateMeshAndMaterial(tileToPlaceInfoEntity, tileType);
        }
    }
}