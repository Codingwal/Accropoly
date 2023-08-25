using System;
using UnityEngine;

public class InputManager : SingletonPersistant<InputManager>
{
    public Controls inputActions;
    public Controls.InGameActions inGameActions;
    private void Awake()
    {
        inputActions = new Controls();
        inGameActions = inputActions.InGame;

        inputActions.Pause.Pause.performed += OnPause;

        GameLoopManager.Instance.GameStateChanged += OnGameStateChanged;
    }
    private void OnDisable() {
        inputActions.Pause.Pause.performed -= OnPause;
    }

    private void OnPause(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        switch (GameLoopManager.Instance.GameState)
        {
            case GameState.InGame:
                GameLoopManager.Instance.GameState = GameState.PauseMenu;
                return;
            case GameState.PauseMenu:
                GameLoopManager.Instance.GameState = GameState.InGame;
                return;
        }
    }

    private void OnGameStateChanged(GameState newGameState, GameState oldGameState)
    {
        switch (newGameState)
        {
            case GameState.MainMenu:
                inputActions.Pause.Disable();
                inGameActions.Disable();
                break;
            case GameState.InGame:
                inputActions.Pause.Enable();
                inGameActions.Enable();
                break;
            case GameState.PauseMenu:
                inputActions.Pause.Enable();
                inGameActions.Disable();
                break;
        }
    }
}