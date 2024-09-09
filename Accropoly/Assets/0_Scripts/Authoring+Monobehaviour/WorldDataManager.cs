using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct WorldDataManager : ISystem
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

        state.EntityManager.RemoveComponent(loadGameTagQuery, typeof(SaveGameTag));
    }
    public static void LoadWorldData()
    {
        Debug.Log("Loading WorldData");

        worldData = SaveSystem.Instance.GetWorldData();

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityManager.CreateSingleton<LoadGameTag>();
        entityManager.CreateSingleton<RunGameTag>();
    }
    public static void SaveWorldData()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityManager.CreateSingleton<SaveGameTag>();
        Debug.Log("Created tag");
    }
}
