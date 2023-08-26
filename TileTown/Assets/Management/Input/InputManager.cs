using System;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    public Controls inputActions;
    public Controls.InGameActions inGameActions;
    public Controls.UIActions uIActions;

    public event Func<bool> PriorityEscape;
    public event Action Escape;

    protected override void Awake()
    {
        base.Awake();

        inputActions = new Controls();
        inGameActions = inputActions.InGame;
        uIActions = inputActions.UI;

        uIActions.Escape.performed += OnEscape;

        GameLoopManager.Instance.GameStateChanged += OnGameStateChanged;
    }

    private void OnEscape(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        bool priorityEscape = PriorityEscape.Invoke();

        if (priorityEscape)
        {
            return;
        }
        Escape.Invoke();
    }

    private void OnGameStateChanged(GameState newGameState, GameState oldGameState)
    {
        switch (newGameState)
        {
            case GameState.MainMenu:
                inputActions.UI.Disable();
                inGameActions.Disable();
                break;
            case GameState.InGame:
                inputActions.UI.Enable();
                inGameActions.Enable();
                break;
            case GameState.PauseMenu:
                inputActions.UI.Enable();
                inGameActions.Disable();
                break;
        }
    }
}
