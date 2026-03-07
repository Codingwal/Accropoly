using System;
using Unity.Entities;
using UnityEngine;
using Components;

public static class MenuUtility
{
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

        WorldManager.CreateWorld();

        InputHandler.EnableInputActions();
        InputHandler.EnableGameplayInputActions();

        continuingGame?.Invoke();
    }
    public static void QuitGame()
    {
        WorldManager.DestroyWorld();

        InputHandler.DisableMenuInputActions();
    }
    public static void DeleteWorld(string mapName)
    {
        FileHandler.DeleteFile("Saves", mapName);
    }
    public static void CreateTemplate(string worldName, string newTemplateName)
    {
        WorldData worldData = SaveSystem.Instance.GetWorldData(worldName);
        SaveSystem.Instance.SaveTemplate(worldData, newTemplateName);
        worldData.Dispose();
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
        WorldManager.PauseWorld();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        InputHandler.DisableGameplayInputActions();
        pausingGame?.Invoke();
    }
    public static void ContinueGame()
    {
        WorldManager.ResumeWorld();

        InputHandler.EnableGameplayInputActions();
        continuingGame?.Invoke();
    }

    public static void OpenExplorer()
    {
        Application.OpenURL(@"file://" + FileHandler.baseDir);
    }
    public static void Quit()
    {
        Application.Quit();
    }
    public static void CreateStandardTemplates()
    {
        var templates = MapTemplates.mapTemplates;
        foreach (var (name, template) in templates)
        {
            SaveSystem.Instance.SaveTemplate(template, name.ToString());
            template.Dispose();
        }
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
    public static void PlaceTile(TileType tileType)
    {
        Systems.BuildingSystem.StartPlacementProcess(tileType);
    }
    private static T GetSingleton<T>() where T : unmanaged, IComponentData
    {
        return EntityManager.CreateEntityQuery(typeof(T)).GetSingleton<T>();
    }
    private static EntityManager EntityManager => ECSUtility.EntityManager;
}
