using Unity.Burst;
using Unity.Entities;
using Components;
using Unity.Rendering;
using Unity.Collections;
using ConfigComponents;

namespace Systems
{
    /// <summary>
    /// Re-create tile grid using the saved tile map data
    /// </summary>
    [UpdateInGroup(typeof(CreationSystemGroup))]
    public partial struct PopulationSetupSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Appearence>();
        }

        public void OnUpdate(ref SystemState state)
        {
            Appearence appearenceConfig = SystemAPI.GetSingleton<Appearence>();

            var tileEntities = SystemAPI.QueryBuilder().WithAll<Person>().WithNone<MaterialMeshInfo>().Build().ToEntityArray(Allocator.Temp);
            foreach (Entity entity in tileEntities)
            {
                AddRenderingComponents(entity, state.EntityManager, appearenceConfig);
            }
        }

        public static void AddRenderingComponents(Entity entity, EntityManager entityManager, Appearence appearenceConfig)
        {
            var renderMeshDesc = new RenderMeshDescription(UnityEngine.Rendering.ShadowCastingMode.On);
            MaterialMeshInfo materialMeshInfo = appearenceConfig.person;

            RenderMeshUtility.AddComponents(entity, entityManager, renderMeshDesc, materialMeshInfo);
        }
    }
}