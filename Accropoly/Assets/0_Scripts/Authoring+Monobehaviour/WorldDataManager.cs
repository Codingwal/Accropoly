using Unity.Entities;
using UnityEngine;

public static class WorldDataManager 
{
    public static WorldData worldData;
    public static void LoadWorldData()
    {
        Debug.Log("Loading WorldData");

        worldData = SaveSystem.Instance.GetWorldData();
    }
    public static void SaveWorldData()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityManager.CreateSingleton<SaveGameTag>();
        Debug.Log("Created tag");

        Debug.Log("Saving WorldData");
        SaveSystem.Instance.SaveWorldData(worldData);
    }
}
