using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Components;

using PlacementAction = Components.PlacementInputData.Action;

[UpdateInGroup(typeof(CreationSystemGroup))]
public partial struct BuildingSystem : ISystem
{
    private EntityQuery placementInputDataQuery;
    private EntityQuery tilePricesQuery;
    private static Entity entity;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RunGameTag>();
        state.RequireForUpdate<TileToPlace>(); // Update only if there is a PlacementProcess running (The process is started by the menu)

        placementInputDataQuery = state.GetEntityQuery(typeof(PlacementInputData));
        tilePricesQuery = state.GetEntityQuery(typeof(ConfigComponents.TilePrices));
    }
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndCreationECBSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        var localTransform = state.EntityManager.GetComponentData<LocalTransform>(entity);
        var tileToPlace = state.EntityManager.GetComponentData<TileToPlace>(entity);

        if (placementInputDataQuery.CalculateEntityCount() != 0) // If there is placementInput (can't use singleton functions bc of IEnableableComponent)
        {
            // The PlacementInputData is attached to the same entity as the singleton InputData
            var placementInputData = state.EntityManager.GetComponentData<PlacementInputData>(SystemAPI.GetSingletonEntity<InputData>());

            if (placementInputData.action == PlacementAction.Rotate)
            {
                tileToPlace.rotation = tileToPlace.rotation.Rotate(1);
                ecb.SetComponent(entity, tileToPlace);
                ecb.SetComponent(entity, localTransform.RotateY(math.radians(90)));
            }
            else if (placementInputData.action == PlacementAction.Cancel)
            {
                ecb.DestroyEntity(entity);
                return;
            }
            else if (placementInputData.action == PlacementAction.Place)
            {
                int2 pos = (int2)localTransform.Position.xz / 2;
                Entity oldTile = TileGridUtility.GetTile(pos);
                TileType newTileType = tileToPlace.tileType;

                // If the tile can be bought, buy it, else, abort
                float price = SystemAPI.ManagedAPI.GetSingleton<ConfigComponents.TilePrices>().prices[newTileType];
                GameInfo info = SystemAPI.GetSingleton<GameInfo>();
                if (price > info.balance) // If the tile can't be bought, abort
                    return;
                info.balance -= price; // Buy the tile
                SystemAPI.SetSingleton(info); // Save the modified GameInfo

                // Set the archetype to the archetype of the newTileType
                var components = TilePlacingUtility.GetComponents(newTileType, pos, tileToPlace.rotation);

                TilePlacingUtility.UpdateEntity(oldTile, components, ecb);

                // Set the transform rotation according to the rotation of tileToPlace
                var transform = state.EntityManager.GetComponentData<LocalTransform>(oldTile);
                transform.Rotation = quaternion.EulerXYZ(0, tileToPlace.rotation.ToRadians(), 0);
                ecb.SetComponent(oldTile, transform);

                // Set mesh & material according to the new tileType
                MaterialsAndMeshesHolder.UpdateMeshAndMaterial(oldTile, newTileType);
            }
        }
    }
    public static void StartPlacementProcess(TileType tileType)
    {
        EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (em.CreateEntityQuery(typeof(TileToPlace)).CalculateEntityCount() != 0) return; // Return if there already is a PlacementProcess

        var prefab = em.CreateEntityQuery(typeof(ConfigComponents.PrefabEntity)).GetSingleton<ConfigComponents.PrefabEntity>(); // Get the tilePrefab
        entity = em.Instantiate(prefab);
        em.AddComponentData(entity, new TileToPlace
        {
            tileType = tileType,
        });
        MaterialsAndMeshesHolder.UpdateMeshAndMaterial(entity, tileType); // Set mesh & material according to the specified tileType
    }
}