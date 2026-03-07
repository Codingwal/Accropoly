using Unity.Entities;
using UnityEngine;

public static class WorldManager
{
    public static World world;
    public static void CreateWorld()
    {
        Debug.Log("Loading world");

        // Create world
        world = new World("My world");
        World.DefaultGameObjectInjectionWorld = world;

        // Load WorldData
        WorldData worldData = SaveSystem.Instance.GetWorldData();
        WorldLoader loader = new(world.EntityManager);
        loader.Load(worldData.worldSave);
        worldData.Dispose();

        // Init tile grid lookup
        InitEntityGrid.CreateEntityGrid(world.EntityManager);

        // Systems must be created after world data loading and setup because OnCreate is executed instantly
        var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
        DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, systems);

        ResumeWorld();
    }
    public static void PauseWorld()
    {
        ScriptBehaviourUpdateOrder.RemoveWorldFromCurrentPlayerLoop(world);
    }
    public static void ResumeWorld()
    {
        ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(world);
    }
    public static void DestroyWorld()
    {
        Debug.Log("Saving and destroying world");

        WorldSaver saver = new(world.EntityManager);
        WorldData worldData = new() { worldSave = saver.Save() };
        SaveSystem.Instance.SaveWorldData(worldData);
        worldData.Dispose();

        world.QuitUpdate = true;
        world.Dispose();

        World.DefaultGameObjectInjectionWorld = null;
    }
}