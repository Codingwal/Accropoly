using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Components;
using PlacementAction = Components.PlacementInputData.Action;
using Tags;
using ConfigComponents;


namespace Systems
{
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
                    var prefab = SystemAPI.GetSingleton<PrefabEntity>();
                    Entity entity = ecb.Instantiate(prefab);
                    ecb.SetComponent(entity, LocalTransform.FromPositionRotation(new(pos.x, 0.3f, pos.y), quaternion.Euler(new(0, tileToPlaceInfo.rotation.ToRadians(), 0))));
                    ecb.AddComponent<TileToPlace>(entity);
                }
            }
            else if (placementInputData.action == PlacementAction.Place)
            {
                var gameInfo = SystemAPI.GetSingleton<GameInfo>();
                float price = SystemAPI.ManagedAPI.GetSingleton<TilePrices>().prices[tileToPlaceInfo.tileType];

                Entities.WithAll<TileToPlace>().ForEach((in LocalTransform transform) =>
                {
                    int2 pos = (int2)transform.Position.xz / 2;
                    Entity tile = TileGridUtility.GetTile(pos);

                    if (price <= gameInfo.balance) // If the tile can be bought
                    {
                        gameInfo.balance -= price; // Buy the tile
                        ecb.AddComponent<Replace>(tile); // Mark tile for placement
                    }
                }).Run();
                ecb.SetComponent(SystemAPI.GetSingletonEntity<GameInfo>(), gameInfo); // Update the balance

                // Delete all TileToPlace entities (as they have been placed now)
                ecb.DestroyEntity(tileToPlaceQuery, EntityQueryCaptureMode.AtPlayback);
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
            }
            else if (placementInputData.action == PlacementAction.Cancel)
            {
                ecb.DestroyEntity(tileToPlaceQuery, EntityQueryCaptureMode.AtPlayback);
                ecb.DestroyEntity(SystemAPI.GetSingletonEntity<TileToPlaceInfo>());
                return;
            }

            // Update meshes and materials (inefficient as it's mostly redundant, but who cares)
            foreach ((_, Entity entity) in SystemAPI.Query<TileToPlace>().WithEntityAccess())
                MaterialsAndMeshesHolder.UpdateMeshAndMaterial(entity, tileToPlaceInfo.tileType);
        }
        public static void StartPlacementProcess(TileType tileType)
        {
            EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var tileToPlaceInfoQuery = em.CreateEntityQuery(typeof(TileToPlaceInfo));

            if (tileToPlaceInfoQuery.IsEmpty) // Is there already a tileToPlaceInfo singleton
            {
                // Create a tileToPlaceInfo singleton entity and set mesh & material
                var prefab = em.CreateEntityQuery(typeof(PrefabEntity)).GetSingleton<PrefabEntity>();
                Entity tileToPlaceInfoEntity = em.Instantiate(prefab);
                em.AddComponentData(tileToPlaceInfoEntity, new TileToPlaceInfo { tileType = tileType });
                MaterialsAndMeshesHolder.UpdateMeshAndMaterial(tileToPlaceInfoEntity, tileType);
            }
            else
            {
                // Update the singleton component and set mesh & material
                Entity tileToPlaceInfoEntity = tileToPlaceInfoQuery.GetSingletonEntity();
                em.SetComponentData(tileToPlaceInfoEntity, new TileToPlaceInfo { tileType = tileType });
                MaterialsAndMeshesHolder.UpdateMeshAndMaterial(tileToPlaceInfoEntity, tileType);
            }

        }
    }
}