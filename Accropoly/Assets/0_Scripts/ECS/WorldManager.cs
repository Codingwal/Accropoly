using Unity.Entities;
using UnityEngine;

public static class WorldManager
{
    public static World world;
    public static void CreateWorld()
    {
        // Create world
        Debug.Log("Creating world");
        world = new World("My world");
        World.DefaultGameObjectInjectionWorld = world;

        // Load WorldData
        Debug.Log("Loading WorldData");
        WorldData worldData = SaveSystem.Instance.GetWorldData();
        WorldLoader loader = new(world.EntityManager);
        loader.Load(worldData.worldSave);

        // Init tile grid lookup
        InitEntityGrid.CreateEntityGrid(world.EntityManager);

        // Systems must be created after world data loading and setup because OnCreate is executed instantly
        var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
        DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, systems);

        ResumeWorld();
    }
    public static void PauseWorld()
    {
        Debug.Log("Pausing world");
        ScriptBehaviourUpdateOrder.RemoveWorldFromCurrentPlayerLoop(world);
    }
    public static void ResumeWorld()
    {
        Debug.Log("Resuming world");
        ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(world);
    }
    public static void DestroyWorld()
    {
        PauseWorld();

        Debug.Log("Saving WorldData");

        WorldSaver saver = new(world.EntityManager);
        WorldData worldData = new() { worldSave = saver.Save() };
        SaveSystem.Instance.SaveWorldData(worldData);


        Debug.Log("Destroying world");

        world.QuitUpdate = true;
        world.Dispose();
    }
}