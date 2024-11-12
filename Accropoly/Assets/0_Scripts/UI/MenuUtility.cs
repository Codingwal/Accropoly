using System;
using Unity.Entities;
using UnityEngine;

public static class MenuUtility
{
    public static Action continuingGame;
    public static Action pausingGame;
    public static void InitUIInfo()
    {
        EntityManager.CreateSingleton<UIInfo>();
    }
    public static UIInfo GetUIInfo()
    {
        return EntityManager.CreateEntityQuery(typeof(UIInfo)).GetSingleton<UIInfo>();
    }
    public static GameInfo GetGameInfo()
    {
        try
        {
            return EntityManager.CreateEntityQuery(typeof(GameInfo)).GetSingleton<GameInfo>();
        }
        catch (InvalidOperationException)
        {
            return default;
        }
    }
    public static void CreateWorld(string worldName, string templateName)
    {
        SaveSystem.Instance.UpdateWorldName(worldName);
        SaveSystem.Instance.CreateWorld(worldName, templateName);
    }
    public static void StartGame(string worldName)
    {
        SaveSystem.Instance.UpdateWorldName(worldName);

        WorldDataSystem.LoadWorldData();

        InputSystem.EnableInputActions();

        ContinueGame();
    }
    public static void QuitGame()
    {
        WorldDataSystem.SaveWorldData();

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityManager.DestroyEntity(entityManager.CreateEntityQuery(typeof(RunGameTag)));

        InputSystem.DisableMenuInputActions();
    }
    public static void DeleteWorld(string mapName)
    {
        FileHandler.DeleteFile("Saves", mapName);
    }
    public static void CreateTemplate(string worldName, string newTemplateName)
    {
        WorldData worldData = SaveSystem.Instance.GetWorldData(worldName);
        SaveSystem.Instance.SaveTemplate(worldData.map, newTemplateName);
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
        Time.timeScale = 0;
        EntityManager.DestroyEntity(EntityManager.CreateEntityQuery(typeof(RunGameTag)));
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        InputSystem.DisableGameplayInputActions();
        pausingGame?.Invoke();
    }
    public static void ContinueGame()
    {
        Time.timeScale = 1;
        EntityManager.CreateSingleton<RunGameTag>();
        InputSystem.EnableGameplayInputActions();
        continuingGame?.Invoke();
    }
    public static void PlaceTile(TileType tileType)
    {
        BuildingSystem.StartPlacementProcess(tileType);
    }
    public static void OpenExplorer()
    {
        Application.OpenURL(@"file://" + FileHandler.baseDir);
    }
    public static void Quit()
    {
        Application.Quit();
    }
    private static EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
    private static InputSystem InputSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<InputSystem>();
}
