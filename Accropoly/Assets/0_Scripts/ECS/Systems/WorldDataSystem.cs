using Unity.Entities;
using UnityEngine;
using Components;
using Tags;
using System.IO;

namespace Systems
{
    /// <summary>
    /// Handle world data saving and loading
    /// (Uses tags to notify data from other systems (PopulationLoadingSystem, TileLoadingSystem, PopulationSavingSystem, TileSavingSystem, ...))
    /// </summary>
    [UpdateInGroup(typeof(PreCreationSystemGroup))]
    public partial struct WorldDataSystem : ISystem
    {
        private static bool loadGame;
        private static bool saveGame;
        public static WorldData worldData;
        private EntityQuery tileMapQuery;
        private EntityQuery populationQuery;
        private EntityQuery gameInfoQuery;
        public void OnCreate(ref SystemState state)
        {
            loadGame = false;
            saveGame = false;

            tileMapQuery = state.GetEntityQuery(typeof(Tile));
            populationQuery = state.GetEntityQuery(typeof(Person));
            gameInfoQuery = state.GetEntityQuery(typeof(GameInfo));
        }
        public void OnUpdate(ref SystemState state)
        {
            // If the game is being saved
            if (SystemAPI.HasSingleton<SaveGame>())
            {
                // Save the worldData
                Debug.Log("Saving WorldData");
                SaveSystem.Instance.SaveWorldData(worldData);

                // Destroy the tag, all people and all tiles
                state.EntityManager.DestroyEntity(tileMapQuery);
                state.EntityManager.DestroyEntity(populationQuery);
                state.EntityManager.DestroyEntity(SystemAPI.GetSingletonEntity<SaveGame>());
            }

            if (SystemAPI.HasSingleton<LoadGame>())
            {
                state.EntityManager.DestroyEntity(SystemAPI.GetSingletonEntity<LoadGame>());

                Debug.Log("Starting game");
                state.EntityManager.CreateSingleton<RunGame>();
            }

            if (loadGame)
            {
                state.EntityManager.CreateSingleton<LoadGame>();
            }

            if (saveGame)
            {
                saveGame = false;
                state.EntityManager.CreateSingleton<SaveGame>();
            }
        }

        public void OnDestroy(ref SystemState state)
        {
            // Dispose waypoint config data

            foreach (var pair in ConfigData.waypointConfig.Data.tiles)
                pair.Value.elements.Dispose();

            foreach (var pair in ConfigData.waypointConfig.Data.elements)
            {
                foreach (var waypoint in pair.Value.waypoints)
                    waypoint.connections.Dispose();

                pair.Value.waypoints.Dispose();
            }

            ConfigData.waypointConfig.Data.tiles.Dispose();
            ConfigData.waypointConfig.Data.elements.Dispose();
        }

        public static void LoadWorldData()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            Debug.Log("Loading WorldData");
            WorldLoader loader = new(entityManager);
            WorldData worldData = SaveSystem.Instance.GetWorldData();
            loader.Load(worldData.worldSave);

            loadGame = true;
        }
        public static void SaveWorldData()
        {
            // saveGame = true;
            WorldSaver saver = new(World.DefaultGameObjectInjectionWorld.EntityManager);
            WorldData worldData = new() { worldSave = saver.Save() };
            SaveSystem.Instance.SaveWorldData(worldData);
        }
    }
}