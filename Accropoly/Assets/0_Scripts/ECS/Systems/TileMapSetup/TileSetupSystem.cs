using Unity.Burst;
using Unity.Entities;
using Components;
using Unity.Rendering;
using Unity.Collections;
using ConfigComponents;
using Unity.Transforms;

namespace Systems
{
    /// <summary>
    /// Re-create tile grid using the saved tile map data
    /// </summary>
    [UpdateInGroup(typeof(CreationSystemGroup))]
    public partial struct TileSetupSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Appearence>();
        }

        public void OnUpdate(ref SystemState state)
        {
            Appearence appearenceConfig = SystemAPI.GetSingleton<Appearence>();

            var tileEntities = SystemAPI.QueryBuilder().WithAll<Tile>().WithNone<MaterialMeshInfo>().Build().ToEntityArray(Allocator.Temp);
            foreach (Entity entity in tileEntities)
            {
                AddRenderingComponents(entity, TileType.Plains, state.EntityManager, appearenceConfig);
            }
        }

        public static void AddRenderingComponents(Entity entity, TileType tileType, EntityManager entityManager, Appearence appearenceConfig)
        {
            var renderMeshDesc = new RenderMeshDescription(UnityEngine.Rendering.ShadowCastingMode.On);
            MaterialMeshInfo materialMeshInfo = AppearenceSystem.GetMaterialMeshInfo(tileType, appearenceConfig);

            RenderMeshUtility.AddComponents(entity, entityManager, renderMeshDesc, materialMeshInfo);

            if (!entityManager.HasComponent<LocalTransform>(entity))
                entityManager.AddComponent<LocalTransform>(entity);
            else
            {
                // This does not change the component but sets it as changed so that WorldRenderBounds gets updated
                var transform = entityManager.GetComponentData<LocalTransform>(entity);
                entityManager.SetComponentData(entity, transform);
            }
        }
    }
}