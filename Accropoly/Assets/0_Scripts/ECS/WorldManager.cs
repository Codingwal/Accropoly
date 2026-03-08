using Unity.Entities;
using UnityEngine;

public static class WorldManager
{
    public static World World { private set; get; } = null;
    public static WorldInfo WorldInfo { private set; get; } = default;

    public static bool ActiveWorld => World != null;

    public static void CreateWorld()
    {
        Debug.Log("Loading world");

        // Create world
        World = new World("My world");
        World.DefaultGameObjectInjectionWorld = World;

        // Get WorldData
        WorldData worldData = SaveSystem.Instance.GetWorldData();

        // Load world
        WorldLoader loader = new(World.EntityManager);
        loader.Load(worldData.worldSave);

        // Set global info
        WorldInfo = new()
        {
            mapSize = worldData.mapSize
        };

        // Dispose WorldData
        worldData.Dispose();

        // Init tile grid lookup
        InitEntityGrid.CreateEntityGrid(World.EntityManager);

        // Systems must be created after world data loading and setup because OnCreate is executed instantly
        var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
        DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(World, systems);

        ResumeWorld();
    }
    public static void PauseWorld()
    {
        ScriptBehaviourUpdateOrder.RemoveWorldFromCurrentPlayerLoop(World);
    }
    public static void ResumeWorld()
    {
        ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(World);
    }
    public static void DestroyWorld()
    {
        Debug.Log("Saving and destroying world");

        WorldSaver saver = new(World.EntityManager);
        WorldData worldData = new(WorldInfo.mapSize, saver.Save());
        SaveSystem.Instance.SaveWorldData(worldData);
        worldData.Dispose();

        World.QuitUpdate = true;
        World.Dispose();

        World.DefaultGameObjectInjectionWorld = null;
        World = null;
    }
}

public struct WorldInfo
{
    public int mapSize;
}