using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoopManager : SingletonPersistant<GameLoopManager>
{
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

        SceneHandler.Instance.SceneIsUnloading += OnSceneIsUnloading;
    }

    private void OnSceneIsUnloading(string newScene)
    {
        if (newScene == "Menu")
        {
            SaveWorld();
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
        Serializable2DArray<TileType> map = DataHandler.Instance.LoadMap();

        MapHandler.Instance.GenerateTileMap(map);
    }
    public void SaveWorld()
    {
        Serializable2DArray<TileType> map = MapHandler.Instance.SaveTileMap();

        DataHandler.Instance.SaveMap(map);
    }
}
public enum GameState
{
    MainMenu,
    InGame,
    PauseMenu
}