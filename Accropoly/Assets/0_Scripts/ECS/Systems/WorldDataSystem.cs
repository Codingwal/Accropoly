using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
public partial struct WorldDataSystem : ISystem
{
    public static WorldData worldData;
    private EntityQuery loadGameTagQuery;
    private EntityQuery saveGameTagQuery;
    private EntityQuery mapTileQuery;
    public void OnCreate(ref SystemState state)
    {
        loadGameTagQuery = state.GetEntityQuery(typeof(LoadGameTag));
        saveGameTagQuery = state.GetEntityQuery(typeof(SaveGameTag));
        mapTileQuery = state.GetEntityQuery(typeof(MapTileComponent));
        state.RequireAnyForUpdate(loadGameTagQuery, saveGameTagQuery);
    }
    public void OnUpdate(ref SystemState state)
    {
        // If the game is being saved
        if (saveGameTagQuery.CalculateEntityCount() != 0)
        {
            // Save the worldData
            Debug.Log("Saving WorldData");
            SaveSystem.Instance.SaveWorldData(worldData);

            // Destroy the tag and all tiles
            state.EntityManager.DestroyEntity(mapTileQuery);
            state.EntityManager.DestroyEntity(saveGameTagQuery);
        }
        // If the game is being loaded
        if (loadGameTagQuery.CalculateEntityCount() != 0)
        {
            // Destroy the tag
            state.EntityManager.DestroyEntity(loadGameTagQuery);
        }
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
