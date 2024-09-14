using System.Collections.Generic;
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

        var localTransform = state.EntityManager.GetComponentData<LocalTransform>(entity);
        var tileToPlace = state.EntityManager.GetComponentData<TileToPlace>(entity);

        if (placementInputDataQuery.CalculateEntityCount() != 0)
        {
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
                int2 pos = (int2)localTransform.Position.xz / 2;
                Entity oldTile = TileGridUtility.GetTile(pos);
                TileType newTileType = tileToPlace.tileType;

                EntityArchetype archetype = state.EntityManager.CreateArchetype(TileTypeToArchetype(newTileType));
                state.EntityManager.SetArchetype(oldTile, archetype);
                state.EntityManager.SetComponentData(oldTile, new MapTileComponent(pos.x, pos.y, newTileType, tileToPlace.rotation));
                MaterialsAndMeshesHolder.UpdateMeshAndMaterial(oldTile, newTileType);
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

        state.EntityManager.SetComponentData(entity, localTransform);
        state.EntityManager.SetComponentData(entity, tileToPlace);
    }
    private static ComponentType[] TileTypeToArchetype(TileType tileType)
    {
        EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity prefab = em.CreateEntityQuery(typeof(TilePrefab)).GetSingleton<TilePrefab>(); // Get the tilePrefab

        List<ComponentType> componentTypes = tileType switch
        {
            TileType.Plains => new() { typeof(AgingTile) },
            TileType.Forest => new() { },
            _ => throw new(),
        };
        componentTypes.Add(typeof(MapTileComponent));


        NativeArray<ComponentType> prefabComponentTypes = em.GetChunk(prefab).Archetype.GetComponentTypes(Allocator.Temp);
        foreach (var e in prefabComponentTypes)
            componentTypes.Add(e);
        prefabComponentTypes.Dispose();

        return componentTypes.ToArray();
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