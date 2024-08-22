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
                GameStateChanged?.Invoke(_gameState, oldGameState);
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
        InputManager.Init();
    }
}
public enum GameState
{
    MainMenu,
    InGame,
    PauseMenu
}