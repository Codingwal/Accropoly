using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Components;
using Tags;

namespace Systems
{
    /// <summary>
    /// Update the position of the TileToPlaceInfo entity (used by BuildingSystem)
    /// </summary>
    public partial struct UpdatePosition : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TileToPlaceInfo>();
        }
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency.Complete();

            var inputData = SystemAPI.GetSingleton<InputData>();
            int mouseRayLayer = LayerMask.GetMask("MouseRayLayer");

            Entity entity = SystemAPI.GetSingletonEntity<TileToPlaceInfo>();
            var localTransform = state.EntityManager.GetComponentData<LocalTransform>(entity);

            Ray ray = Camera.main.ScreenPointToRay(inputData.mousePos);

            if (!Physics.Raycast(ray, out RaycastHit info, 1000, mouseRayLayer)) // Cast ray to detect where on the map the user points
            {
                SystemAPI.SetComponentEnabled<MaterialMeshInfo>(entity, false); // Hide the TileToPlaceInfo entity if the user doesn't point on the tileMap
                return;
            }

            SystemAPI.SetComponentEnabled<MaterialMeshInfo>(entity, true); // Show the TileToPlaceInfo entity if the user points on the tileMap

            localTransform.Position.xz = math.round(((float3)info.point).xz / 2) * 2; // Align the position to the tileGrid
            localTransform.Position.y = 0.5f; // Important for tile visibility

            // Update the components
            SystemAPI.SetComponent(entity, localTransform);
        }
    }
}