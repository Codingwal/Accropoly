using Unity.Entities;
using UnityEngine;
using Components;
using Tags;

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
                // Save GameInfo
                GameInfo gameInfo = SystemAPI.GetSingleton<GameInfo>();
                worldData.balance = gameInfo.balance;
                worldData.time = gameInfo.time;
                state.EntityManager.DestroyEntity(gameInfoQuery);

                // Save the worldData
                Debug.Log("Saving WorldData");
                SaveSystem.Instance.SaveWorldData(worldData);

                // Destroy the tag, all people and all tiles
                state.EntityManager.DestroyEntity(tileMapQuery);
                state.EntityManager.DestroyEntity(populationQuery);
                state.EntityManager.DestroyEntity(SystemAPI.GetSingletonEntity<SaveGame>());
            }

            // If the game has been loaded
            if (SystemAPI.HasSingleton<LoadGame>())
            {
                state.EntityManager.DestroyEntity(SystemAPI.GetSingletonEntity<LoadGame>());
                state.EntityManager.CreateSingleton<RunGame>();
            }

            if (loadGame)
            {
                loadGame = false;

                Debug.Log("Loading WorldData");

                worldData = SaveSystem.Instance.GetWorldData();
                state.EntityManager.CreateSingleton(new GameInfo
                {
                    balance = worldData.balance,
                    time = worldData.time,
                });
                state.EntityManager.CreateSingleton<LoadGame>();
            }
            if (saveGame)
            {
                saveGame = false;
                state.EntityManager.CreateSingleton<SaveGame>();
            }
        }
        public static void LoadWorldData()
        {
            loadGame = true;
        }
        public static void SaveWorldData()
        {
            saveGame = true;
        }
    }
}