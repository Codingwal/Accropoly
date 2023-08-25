using System;

public class GameLoopManager : Singleton<GameLoopManager>
{
    public event Action<GameState> GameStateChanged;
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
            GameStateChanged?.Invoke(oldGameState);
        }
    }
    private void Awake()
    {
        GameState = GameState.MainMenu;
        GameStateChanged += OnGameStateChanged;

        FileHandler.Init();
    }

    private void OnGameStateChanged(GameState oldGameState)
    {
        if (oldGameState == GameState.MainMenu && GameState == GameState.InGame)
        {
            LoadWorld();
        }
    }

    private void OnApplicationQuit()
    {
        SaveWorld();
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