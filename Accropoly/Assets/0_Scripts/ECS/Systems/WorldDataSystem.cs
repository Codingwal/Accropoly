using Unity.Entities;
using UnityEngine;
using Components;

namespace Systems
{
    [UpdateInGroup(typeof(PreCreationSystemGroup))]
    public partial struct WorldDataSystem : ISystem
    {
        private static bool loadGame;
        private static bool saveGame;
        public static WorldData worldData;
        private EntityQuery loadGameTagQuery;
        private EntityQuery saveGameTagQuery;
        private EntityQuery tileMapQuery;
        private EntityQuery populationQuery;
        private EntityQuery gameInfoQuery;
        public void OnCreate(ref SystemState state)
        {
            loadGame = false;
            saveGame = false;

            loadGameTagQuery = state.GetEntityQuery(typeof(Tags.LoadGame));
            saveGameTagQuery = state.GetEntityQuery(typeof(Tags.SaveGame));
            tileMapQuery = state.GetEntityQuery(typeof(Tile));
            populationQuery = state.GetEntityQuery(typeof(Person));
            gameInfoQuery = state.GetEntityQuery(typeof(GameInfo));
        }
        public void OnUpdate(ref SystemState state)
        {
            // If the game is being saved
            if (saveGameTagQuery.CalculateEntityCount() != 0)
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
                state.EntityManager.DestroyEntity(saveGameTagQuery);
            }

            // If the game has been loaded
            if (loadGameTagQuery.CalculateEntityCount() != 0)
            {
                state.EntityManager.DestroyEntity(loadGameTagQuery);
                state.EntityManager.CreateSingleton<Tags.RunGame>();
            }

            if (loadGame)
            {
                loadGame = false;

                Debug.Log("Loading WorldData");

                worldData = SaveSystem.Instance.GetWorldData();
                World.DefaultGameObjectInjectionWorld.EntityManager.CreateSingleton(new GameInfo
                {
                    balance = worldData.balance,
                    time = worldData.time,
                });
                state.EntityManager.CreateSingleton<Tags.LoadGame>();
            }
            if (saveGame)
            {
                saveGame = false;
                state.EntityManager.CreateSingleton<Tags.SaveGame>();
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