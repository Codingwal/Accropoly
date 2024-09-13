using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using PlacementAction = PlacementInputData.Action;
public partial struct BuildingSystem : ISystem
{
    private EntityQuery placementInputDataQuery;
    private static Entity entity;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RunGameTag>();
        state.RequireForUpdate<TileToPlace>();
        state.RequireForUpdate<BuildingSystemConfig>();

        placementInputDataQuery = state.GetEntityQuery(typeof(PlacementInputData));
    }
    public void OnUpdate(ref SystemState state)
    {
        if (Time.timeScale == 0) return; // Return if the game is paused

        var config = SystemAPI.GetSingleton<BuildingSystemConfig>();

        var tileToPlace = SystemAPI.GetSingleton<TileToPlace>();

        if (placementInputDataQuery.CalculateEntityCount() != 0)
        {
            var placementInputData = state.EntityManager.GetComponentData<PlacementInputData>(SystemAPI.GetSingletonEntity<InputData>());

            if (placementInputData.action == PlacementAction.Rotate)
            {
                // Rotate tileToPlace
            }
            else if (placementInputData.action == PlacementAction.Cancel)
            {
                state.EntityManager.DestroyEntity(entity);
                return;
            }
            else if (placementInputData.action == PlacementAction.Place)
            {
                // Place tileToPlace
            }
        }

        // Update position

        var inputData = SystemAPI.GetSingleton<InputData>();

        Ray ray = Camera.main.ScreenPointToRay(inputData.mousePos);

        if (!Physics.Raycast(ray, out RaycastHit info, 1000, config.mouseRayLayer)) // Cast ray to detect where on the map the user points
        {
            state.EntityManager.SetComponentEnabled<MaterialMeshInfo>(entity, false); // Hide the tileToPlace entity if the user doesn't point on the tileMap
            return;
        };
        state.EntityManager.SetComponentEnabled<MaterialMeshInfo>(entity, true); // Show the tileToPlace entity if the user points on the tileMap

        var transform = LocalTransform.FromPosition(info.point);
        transform.Position.xz = math.round((transform.Position.xz + 1) / 2) * 2 - 1; // Align the position to the tileGrid
        transform.Position.y = 0.5f; // Important for tile visibility
        state.EntityManager.SetComponentData(entity, transform);
    }
    public static void StartPlacementProcess(TileType tileType)
    {
        EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (em.CreateEntityQuery(typeof(TileToPlace)).CalculateEntityCount() != 0) return; // Return if there already is a PlacementProcess

        var prefab = em.CreateEntityQuery(typeof(TilePrefab)).GetSingleton<TilePrefab>(); // Get the tilePrefab
        entity = em.Instantiate(prefab);
        em.AddComponentData(entity, new TileToPlace
        {
            tileType = tileType,
        });
        MaterialsAndMeshesHolder.UpdateMeshAndMaterial(entity, tileType); // Set mesh & material according to the specified tileType
    }
}

