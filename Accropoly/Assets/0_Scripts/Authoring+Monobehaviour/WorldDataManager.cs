using System.Threading.Tasks;
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
        World.DefaultGameObjectInjectionWorld.EntityManager.CreateSingleton<LoadGameTag>();
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
    private async void SaveWorldData()
    {

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityManager.CreateSingleton<SaveGameTag>();
        Debug.Log("Created tag");

        await Task.Delay(10);

        Debug.Log("Saving WorldData");
        SaveSystem.Instance.SaveWorldData(worldData);
    }
}
