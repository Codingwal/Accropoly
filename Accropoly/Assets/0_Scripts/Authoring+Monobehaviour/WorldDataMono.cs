using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldDataMono : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Loading WorldData");

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        WorldData data = SaveSystem.Instance.GetWorldData();

        Entity worldDataHolder = entityManager.CreateSingleton<WorldData>();
        entityManager.SetComponentData(worldDataHolder, data);

        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == "Game")
            SaveWorldData();
    }

    private void OnApplicationQuit()
    {
        SaveWorldData();
    }
    private void SaveWorldData()
    {
        Debug.Log("Saving WorldData");

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        Entity worldDataHolder = entityManager.CreateEntityQuery(typeof(WorldData)).GetSingletonEntity();
        WorldData worldData = entityManager.GetComponentData<WorldData>(worldDataHolder);

        SaveSystem.Instance.SaveWorldData(worldData);

        worldData.population.Dispose();
        worldData.map.tiles.Dispose();


        entityManager.DestroyEntity(worldDataHolder);
    }
}
