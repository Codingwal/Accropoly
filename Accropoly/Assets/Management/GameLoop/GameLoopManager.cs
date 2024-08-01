using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public delegate void RefAction<T>(ref T obj);
public class GameLoopManager : SingletonPersistant<GameLoopManager>
{
    // Time and invoice system
    [Header("Time and invoice system")]
    [SerializeField] private float timeSpeed;
    public float invoiceInterval;
    private float nextInvoiceTime;
    public float playTime;

    // Loading and saving
    public event Action<World> InitWorld;
    public event RefAction<World> SaveWorld;

    // State machine
    public event Action<GameState, GameState> GameStateChanged;
    private GameState _gameState;
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
    protected override void Awake()
    {
        base.Awake();

        if (this == null)
        {
            return;
        }

        FileHandler.Init();
    }
    private void OnEnable()
    {
        SceneManagement.SceneIsUnloading += OnSceneIsUnloading;
    }
    private void OnDisable()
    {
        SceneManagement.SceneIsUnloading -= OnSceneIsUnloading;
    }
    private void OnApplicationQuit()
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            return;
        }
        SaveWorldData();
    }
    // ------------------------------------------------------------------- //
    private void OnSceneIsUnloading(string newScene)
    {
        if (newScene == "Menu")
        {
            SaveWorldData();
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
                await SceneManagement.LoadScene("Menu");
                break;
        }
        GameStateChanged?.Invoke(_gameState, oldGameState);
    }

    private async Task SwitchToGame()
    {
        await SceneManagement.LoadScene("Game");
        LoadWorld();
    }

    private void SaveWorldData()
    {
        Serializable2DArray<Tile> map = MapHandler.Instance.SaveTileMap();

        World world = new(map)
        {
            playTime = playTime
        };

        SaveWorld.Invoke(ref world);

        FileHandler.SaveWorld(world);
    }
    public void LoadWorld()
    {
        World world = FileHandler.LoadWorld();

        playTime = world.playTime;

        nextInvoiceTime = 0;
        while (playTime > nextInvoiceTime)
        {
            nextInvoiceTime += invoiceInterval;
        }

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