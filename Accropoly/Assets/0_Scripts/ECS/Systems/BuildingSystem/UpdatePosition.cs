using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Components;

public partial struct UpdatePosition : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TileToPlace>();
        state.RequireForUpdate<ConfigComponents.BuildingSystem>();
        state.RequireForUpdate<RunGameTag>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        Entity entity = SystemAPI.GetSingletonEntity<TileToPlace>();
        var localTransform = state.EntityManager.GetComponentData<LocalTransform>(entity);

        var inputData = SystemAPI.GetSingleton<InputData>();
        var config = SystemAPI.GetSingleton<ConfigComponents.BuildingSystem>();

        Ray ray = Camera.main.ScreenPointToRay(inputData.mousePos);

        if (!Physics.Raycast(ray, out RaycastHit info, 1000, config.mouseRayLayer)) // Cast ray to detect where on the map the user points
        {
            ecb.SetComponentEnabled<MaterialMeshInfo>(entity, false); // Hide the tileToPlace entity if the user doesn't point on the tileMap
            return;
        };
        ecb.SetComponentEnabled<MaterialMeshInfo>(entity, true); // Show the tileToPlace entity if the user points on the tileMap

        localTransform.Position.xz = math.round(((float3)info.point).xz / 2) * 2; // Align the position to the tileGrid
        localTransform.Position.y = 0.5f; // Important for tile visibility

        // Update the components
        ecb.SetComponent(entity, localTransform);
    }
}
