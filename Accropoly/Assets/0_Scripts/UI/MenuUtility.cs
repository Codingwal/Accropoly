using System;
using Unity.Entities;
using UnityEngine;

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
        InputSystem.DisableGameplayInputActions();
        pausingGame?.Invoke();
    }
    public static void ContinueGame()
    {
        Time.timeScale = 1;
        InputSystem.EnableGameplayInputActions();
        continuingGame?.Invoke();
    }
    public static void PlaceTile(TileType tileType)
    {
        BuildingSystem.StartPlacementProcess(tileType);
    }
    private static InputSystem InputSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<InputSystem>();
}