using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

using PlacementAction = PlacementInputData.Action;
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct BuildingSystem : ISystem
{
    private EntityQuery placementInputDataQuery;
    private static Entity entity;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RunGameTag>();
        state.RequireForUpdate<TileToPlace>(); // Update only if there is a PlacementProcess running (The process is started by the menu)
        state.RequireForUpdate<BuildingSystemConfig>();

        placementInputDataQuery = state.GetEntityQuery(typeof(PlacementInputData));
    }
    public void OnUpdate(ref SystemState state)
    {
        if (Time.timeScale == 0) return; // Return if the game is paused

        var config = SystemAPI.GetSingleton<BuildingSystemConfig>();

        var localTransform = state.EntityManager.GetComponentData<LocalTransform>(entity);
        var tileToPlace = state.EntityManager.GetComponentData<TileToPlace>(entity);

        if (placementInputDataQuery.CalculateEntityCount() != 0) // If there is placementInput (can't use singleton functions bc of IEnableableComponent)
        {
            // The PlacementInputData is attached to the same entity as the singleton InputData
            var placementInputData = state.EntityManager.GetComponentData<PlacementInputData>(SystemAPI.GetSingletonEntity<InputData>());

            if (placementInputData.action == PlacementAction.Rotate)
            {
                tileToPlace.Rotate(90);
                localTransform = localTransform.RotateY(math.radians(90));
            }
            else if (placementInputData.action == PlacementAction.Cancel)
            {
                state.EntityManager.DestroyEntity(entity);
                return;
            }
            else if (placementInputData.action == PlacementAction.Place)
            {
                EntityCommandBuffer ecb = new(Allocator.Temp);

                int2 pos = (int2)localTransform.Position.xz / 2;
                TileType newTileType = tileToPlace.tileType;

                Entity oldTile = TileGridUtility.GetTile(pos);
                ecb.DestroyEntity(oldTile);

                var components = TilePlacingUtility.GetComponents(newTileType, pos, tileToPlace.rotation);
                Entity newTile = TilePlacingUtility.CreateTile(components, ecb);

                ecb.Playback(state.EntityManager);
                ecb.Dispose();

                // Set the transform rotation according to the rotation of tileToPlace
                var transform = state.EntityManager.GetComponentData<LocalTransform>(newTile);
                transform.Rotation = quaternion.EulerXYZ(0, math.radians(tileToPlace.rotation), 0);
                state.EntityManager.SetComponentData(newTile, transform);

                MaterialsAndMeshesHolder.UpdateMeshAndMaterial(newTile, newTileType); // Update the mesh according to the newTileType 
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

        localTransform.Position.xz = math.round(((float3)info.point).xz / 2) * 2; // Align the position to the tileGrid
        localTransform.Position.y = 0.5f; // Important for tile visibility

        // Update the components
        state.EntityManager.SetComponentData(entity, localTransform);
        state.EntityManager.SetComponentData(entity, tileToPlace);
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