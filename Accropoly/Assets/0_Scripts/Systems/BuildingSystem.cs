using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using PlacementAction = PlacementInputData.Action;
public partial struct BuildingSystem : ISystem
{
    EntityQuery placementInputDataQuery;
    Entity entity;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RunGameTag>();
        // state.RequireForUpdate<TileToPlace>();
        state.RequireForUpdate<BuildingSystemConfig>();

        placementInputDataQuery = state.GetEntityQuery(typeof(PlacementInputData));

        entity.Index = -1;
    }
    public void OnUpdate(ref SystemState state)
    {
        if (Time.timeScale == 0) return;

        var config = SystemAPI.GetSingleton<BuildingSystemConfig>();

        if (entity.Index == -1)
        {
            var prefab = SystemAPI.GetSingleton<TilePrefab>();
            entity = state.EntityManager.Instantiate(prefab);
        }

        // bool placementInput = placementInputDataQuery.CalculateEntityCount() != 0;
        // var tileToPlace = SystemAPI.GetSingleton<TileToPlace>();

        // if (placementInput)
        // {
        //     var placementInputData = placementInputDataQuery.GetSingleton<PlacementInputData>(); // TODO: Can't use GetSingleton
        //     if (placementInputData.action == PlacementAction.Rotate)
        //     {
        //         // Rotate tileToPlace
        //     }
        //     else if (placementInputData.action == PlacementAction.Cancel)
        //     {
        //         // Cancel placing process
        //     }
        //     else if (placementInputData.action == PlacementAction.Place)
        //     {
        //         // Place tileToPlace
        //     }
        // }

        var inputData = SystemAPI.GetSingleton<InputData>();

        // Shoot ray, update tileToPlace pos

        Ray ray = Camera.main.ScreenPointToRay(inputData.mousePos);

        if (!Physics.Raycast(ray, out RaycastHit info, 1000, config.mouseRayLayer))
        {
            state.EntityManager.SetComponentEnabled<MaterialMeshInfo>(entity, false);
            return;
        };
        state.EntityManager.SetComponentEnabled<MaterialMeshInfo>(entity, true);

        var transform = LocalTransform.FromPosition(info.point);
        transform.Position.xz = math.round((transform.Position.xz + 1) / 2) * 2 - 1;
        transform.Scale = 2;
        state.EntityManager.SetComponentData(entity, transform);
    }
}

