using System;
using Unity.Entities;
using UnityEngine;
using Components;

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
        return GetSingleton<UIInfo>();
    }
    public static GameInfo GetGameInfo()
    {
        try
        {
            return GetSingleton<GameInfo>();
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

        Systems.WorldDataSystem.LoadWorldData();

        InputSystem.EnableInputActions();

        Time.timeScale = 1;
        InputSystem.EnableGameplayInputActions();
        continuingGame?.Invoke();
    }
    public static void QuitGame()
    {
        Systems.WorldDataSystem.SaveWorldData();

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityManager.DestroyEntity(entityManager.CreateEntityQuery(typeof(Tags.RunGame)));

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
        EntityManager.DestroyEntity(EntityManager.CreateEntityQuery(typeof(Tags.RunGame)));
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        InputSystem.DisableGameplayInputActions();
        pausingGame?.Invoke();
    }
    public static void ContinueGame()
    {
        Time.timeScale = 1;
        EntityManager.CreateSingleton<Tags.RunGame>();
        InputSystem.EnableGameplayInputActions();
        continuingGame?.Invoke();
    }
    public static void PlaceTile(TileType tileType)
    {
        Systems.BuildingSystem.StartPlacementProcess(tileType);
    }
    public static void OpenExplorer()
    {
        Application.OpenURL(@"file://" + FileHandler.baseDir);
    }
    public static void Quit()
    {
        Application.Quit();
    }
    private static T GetSingleton<T>() where T : unmanaged, IComponentData
    {
        return EntityManager.CreateEntityQuery(typeof(T)).GetSingleton<T>();
    }
    private static EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
    private static Systems.InputSystem InputSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Systems.InputSystem>();
}
