using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoopManager : SingletonPersistant<GameLoopManager>
{
    public delegate void RefAction<T>(ref T obj);
    public event Action<World> InitWorld;
    public event RefAction<World> SaveWorld;

    public event Action<GameState, GameState> GameStateChanged;
    public GameState _gameState;
    public GameState GameState
    {
        get
        {
            return _gameState;
        }
        set
        {
            GameState oldGameState = _gameState;
            _gameState = value;
            if (oldGameState != _gameState)
            {
                OnGameStateChanged(_gameState, oldGameState);
            }
        }
    }
    private void Awake()
    {
        FileHandler.Init();
    }
    private void OnEnable()
    {
        SceneHandler.Instance.SceneIsUnloading += OnSceneIsUnloading;
    }
    private void OnDisable()
    {
        SceneHandler.Instance.SceneIsUnloading -= OnSceneIsUnloading;
    }

    private void OnSceneIsUnloading(string newScene)
    {
        if (newScene == "Menu")
        {
            Serializable2DArray<TileType> map = MapHandler.Instance.SaveTileMap();

            World world = DataHandler.Instance.LoadWorld();

            world.map = map;
            SaveWorld.Invoke(ref world);

            DataHandler.Instance.SaveWorld(world);
        }
    }

    private async void OnGameStateChanged(GameState newGameState, GameState oldGameState)
    {
        switch (newGameState)
        {
            case GameState.InGame:
                if (oldGameState == GameState.MainMenu)
                {
                    await SwitchToGame();
                }
                break;
            case GameState.MainMenu:
                await SceneHandler.Instance.LoadScene("Menu");
                break;
        }
        GameStateChanged?.Invoke(_gameState, oldGameState);
    }

    private async Task SwitchToGame()
    {
        await SceneHandler.Instance.LoadScene("Game");
        LoadWorld();
    }

    public void LoadWorld()
    {
        World world = DataHandler.Instance.LoadWorld();

        MapHandler.Instance.GenerateTileMap(world.map);
        InitWorld.Invoke(world);
    }
}
public enum GameState
{
    MainMenu,
    InGame,
    PauseMenu
}