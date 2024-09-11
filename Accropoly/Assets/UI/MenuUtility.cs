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

        WorldDataSystem.LoadWorldData();

        GetInputSystem().EnableInputActions();
    }
    public static void QuitGame()
    {
        WorldDataSystem.SaveWorldData();

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityManager.DestroyEntity(entityManager.CreateEntityQuery(typeof(RunGameTag)));
        entityManager.DestroyEntity(entityManager.CreateEntityQuery(typeof(MapTileComponent)));

        GetInputSystem().DisableMenuInputActions();
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

        GetInputSystem().DisableGameplayInputActions();
        pausingGame?.Invoke();
    }
    public static void ContinueGame()
    {
        // TODO: Continue game

        GetInputSystem().EnableGameplayInputActions();
        continuingGame?.Invoke();
    }
    private static InputSystem GetInputSystem()
    {
        return World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<InputSystem>();
    }
}
