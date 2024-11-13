using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(LateInitializationSystemGroup))]
public partial struct WorldDataSystem : ISystem
{
    public static WorldData worldData;
    private EntityQuery loadGameTagQuery;
    private EntityQuery saveGameTagQuery;
    private EntityQuery tileMapQuery;
    private EntityQuery populationQuery;
    private EntityQuery gameInfoQuery;
    public void OnCreate(ref SystemState state)
    {
        loadGameTagQuery = state.GetEntityQuery(typeof(LoadGameTag));
        saveGameTagQuery = state.GetEntityQuery(typeof(SaveGameTag));
        tileMapQuery = state.GetEntityQuery(typeof(MapTileComponent));
        populationQuery = state.GetEntityQuery(typeof(PersonComponent));
        gameInfoQuery = state.GetEntityQuery(typeof(GameInfo));
        state.RequireAnyForUpdate(loadGameTagQuery, saveGameTagQuery);
    }
    public void OnUpdate(ref SystemState state)
    {
        // If the game is being saved
        if (saveGameTagQuery.CalculateEntityCount() != 0)
        {
            // Save GameInfo
            GameInfo gameInfo = SystemAPI.GetSingleton<GameInfo>();
            worldData.balance = gameInfo.balance;
            worldData.time = gameInfo.time;
            state.EntityManager.DestroyEntity(gameInfoQuery);

            // Save the worldData
            Debug.Log("Saving WorldData");
            SaveSystem.Instance.SaveWorldData(worldData);

            // Destroy the tag, all people and all tiles
            state.EntityManager.DestroyEntity(tileMapQuery);
            state.EntityManager.DestroyEntity(populationQuery);
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
        World.DefaultGameObjectInjectionWorld.EntityManager.CreateSingleton(new GameInfo
        {
            balance = worldData.balance,
            time = worldData.time,
        });

        CreateTag<LoadGameTag>();
    }
    public static void SaveWorldData()
    {
        CreateTag<SaveGameTag>();
    }
    private static void CreateTag<T>() where T : unmanaged, IComponentData
    {
        World.DefaultGameObjectInjectionWorld.EntityManager.CreateSingleton<T>();
    }
}