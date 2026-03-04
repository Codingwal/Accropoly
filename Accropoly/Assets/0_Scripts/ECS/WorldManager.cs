using Unity.Entities;
using UnityEngine;

public static class WorldManager
{
    public static World world;
    public static void CreateWorld()
    {
        Debug.Log("Creating world");

        world = new World("My world");

        var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);

        World.DefaultGameObjectInjectionWorld = world;

        DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, systems);

        ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(world);


        Debug.Log("Loading WorldData");

        WorldData worldData = SaveSystem.Instance.GetWorldData();
        WorldLoader loader = new(world.EntityManager);
        loader.Load(worldData.worldSave);
    }
    public static void DestroyWorld()
    {
        Debug.Log("Saving WorldData");

        WorldSaver saver = new(world.EntityManager);
        WorldData worldData = new() { worldSave = saver.Save() };
        SaveSystem.Instance.SaveWorldData(worldData);


        Debug.Log("Destroying world");

        world.QuitUpdate = true;
        world.Dispose();
    }
}