using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldDataManager : MonoBehaviour
{
    public static WorldData worldData;
    public static ActionRef<WorldData> onWorldDataLoaded;
    public static ActionRef<WorldData> onWorldDataSaving;
    private void Start()
    {
        LoadWorldData();
        World.DefaultGameObjectInjectionWorld.EntityManager.CreateSingleton<RunGameTag>();
    }
    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
    private void OnApplicationQuit()
    {
        SaveWorldData();
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == "Game")
            SaveWorldData();
    }
    private void LoadWorldData()
    {
        Debug.Log("Loading WorldData");

        worldData = SaveSystem.Instance.GetWorldData();

        onWorldDataLoaded?.Invoke(ref worldData);
    }
    private void SaveWorldData()
    {
        Debug.Log("Saving WorldData");

        onWorldDataSaving?.Invoke(ref worldData);

        SaveSystem.Instance.SaveWorldData(worldData);
    }
}
