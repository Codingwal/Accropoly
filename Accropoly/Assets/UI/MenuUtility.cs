using System;
using Unity.Entities;

public static class MenuUtility
{
    public static Action<float> loadingWorld; // TODO: call every frame while world is loading
    public static Action continuingGame;
    public static Action pausingGame;
    public static void CreateWorld(string worldName, string templateName)
    {
        SaveSystem.Instance.UpdateWorldName(worldName);
        SaveSystem.Instance.CreateWorld(worldName, templateName);
    }
    public static void StartGame(string worldName)
    {
        SaveSystem.Instance.UpdateWorldName(worldName);

        WorldDataManager.LoadWorldData();

        InputSystem.Instance.EnableInputActions();
    }
    public static void QuitGame()
    {
        WorldDataManager.SaveWorldData();

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityManager.DestroyEntity(entityManager.CreateEntityQuery(typeof(RunGameTag)));
    }
    public static void DeleteWorld(string mapName)
    {
        FileHandler.DeleteFile("Saves", mapName);
    }
    public static string[] GetMapTemplateNames()
    {
        return FileHandler.ListFiles("Templates");
    }
    public static string[] GetWorldNames()
    {
        return FileHandler.ListFiles("Saves");
    }
    public static void PauseGame()
    {
        // TODO: Pause game

        InputSystem.Instance.DisableInputActions();
        pausingGame?.Invoke();
    }
    public static void ContinueGame()
    {
        // TODO: Continue game

        InputSystem.Instance.EnableInputActions();
        continuingGame?.Invoke();
    }
}
