using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
public partial struct WorldDataSystem : ISystem
{
    public static WorldData worldData;
    private EntityQuery loadGameTagQuery;
    private EntityQuery saveGameTagQuery;
    public void OnCreate(ref SystemState state)
    {
        loadGameTagQuery = state.GetEntityQuery(typeof(LoadGameTag));
        saveGameTagQuery = state.GetEntityQuery(typeof(SaveGameTag));
        state.RequireAnyForUpdate(loadGameTagQuery, saveGameTagQuery);
    }
    public void OnUpdate(ref SystemState state)
    {
        state.EntityManager.RemoveComponent(loadGameTagQuery, typeof(LoadGameTag));

        if (saveGameTagQuery.CalculateEntityCount() != 0)
        {
            Debug.Log("Saving WorldData");
            SaveSystem.Instance.SaveWorldData(worldData);
        }

        state.EntityManager.RemoveComponent(saveGameTagQuery, typeof(SaveGameTag));
    }
    public static void LoadWorldData()
    {
        Debug.Log("Loading WorldData");

        worldData = SaveSystem.Instance.GetWorldData();

        CreateTag<LoadGameTag>();
        CreateTag<RunGameTag>();
    }
    public static void SaveWorldData()
    {
        CreateTag<SaveGameTag>();
    }
    private static void CreateTag<T>()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        EntityQueryBuilder queryDesc = new EntityQueryBuilder(Allocator.Temp)
                                            .WithAll<BeginInitializationEntityCommandBufferSystem.Singleton>()
                                            .WithOptions(EntityQueryOptions.IncludeSystems);
        EntityQuery query = entityManager.CreateEntityQuery(queryDesc);

        EntityCommandBuffer ecb = query.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(entityManager.WorldUnmanaged);

        var entity = ecb.CreateEntity();
        ecb.AddComponent(entity, typeof(T));
    }
}
