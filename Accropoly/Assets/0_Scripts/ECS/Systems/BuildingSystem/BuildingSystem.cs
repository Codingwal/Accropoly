using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Components;
using PlacementAction = Components.PlacementInputData.Action;
using Tags;
using Unity.Collections;


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
            RequireForUpdate<TileToPlace>(); // Update only if there is a PlacementProcess running (The process is started by the menu)

            placementInputDataQuery = GetEntityQuery(typeof(PlacementInputData));
            tileToPlaceQuery = GetEntityQuery(typeof(TileToPlace));
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndCreationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            if (!placementInputDataQuery.IsEmpty) // If there is placementInput (can't use singleton functions bc of IEnableableComponent)
            {
                // The PlacementInputData is attached to the same entity as the singleton InputData
                var placementInputData = EntityManager.GetComponentData<PlacementInputData>(SystemAPI.GetSingletonEntity<InputData>());
                var tileToPlaceInfo = SystemAPI.GetSingleton<TileToPlaceInfo>();

                if (placementInputData.action == PlacementAction.Rotate)
                {
                    tileToPlaceInfo.rotation = tileToPlaceInfo.rotation.Rotate(1);
                    ecb.SetComponent(SystemAPI.GetSingletonEntity<TileToPlaceInfo>(), tileToPlaceInfo);
                    Entities.WithAll<TileToPlace>().ForEach((ref LocalTransform transform) =>
                    {
                        transform = transform.RotateY(math.radians(90));
                    }).Schedule();
                }
                else if (placementInputData.action == PlacementAction.Cancel)
                {
                    ecb.DestroyEntity(tileToPlaceQuery, EntityQueryCaptureMode.AtPlayback);
                    ecb.DestroyEntity(SystemAPI.GetSingletonEntity<TileToPlaceInfo>());
                    return;
                }
                else if (placementInputData.action == PlacementAction.Place)
                {
                    var gameInfo = SystemAPI.GetSingleton<GameInfo>();
                    float price = SystemAPI.ManagedAPI.GetSingleton<ConfigComponents.TilePrices>().prices[tileToPlaceInfo.tileType];

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
                }
            }
        }
        public static void StartPlacementProcess(TileType tileType)
        {
            EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;

            if (tileToPlaceQuery.IsEmpty)
            {
                var prefab = em.CreateEntityQuery(typeof(ConfigComponents.PrefabEntity)).GetSingleton<ConfigComponents.PrefabEntity>(); // Get the tilePrefab
                Entity entity = em.Instantiate(prefab);
                em.AddComponent<TileToPlace>(entity);

                em.CreateEntity(typeof(TileToPlaceInfo));
            }

            Entity tileToPlaceInfoSingletonEntity = em.CreateEntityQuery(typeof(TileToPlaceInfo)).GetSingletonEntity();
            em.SetComponentData(tileToPlaceInfoSingletonEntity, new TileToPlaceInfo { tileType = tileType });

            var tileToPlaceEntities = tileToPlaceQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity entity in tileToPlaceEntities)
                MaterialsAndMeshesHolder.UpdateMeshAndMaterial(entity, tileType); // Set mesh & material according to the specified tileType
            tileToPlaceEntities.Dispose();
        }
    }
}