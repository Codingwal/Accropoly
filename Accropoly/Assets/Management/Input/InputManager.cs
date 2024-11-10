using System;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class InputManager : Singleton<InputManager>
{
    public Controls inputActions;
    public Controls.InGameActions inGameActions;
    public Controls.UIActions uIActions;

    public event Func<bool> PriorityEscape;
    public event Action Escape;
    public event Action MenuToggled;

    public event Action Hotkey1;
    public event Action Hotkey2;
    public event Action Hotkey3;
    public event Action Hotkey4;
    public event Action Hotkey5;
    public event Action Hotkey6;

    public event Action MenuHotkey1;
    public event Action MenuHotkey2;
    public event Action MenuHotkey3;
    public event Action MenuHotkey4;
    public event Action MenuHotkey5;
    public event Action MenuHotkey6;

    protected override void Awake()
    {
        base.Awake();

        inputActions = new Controls();
        inGameActions = inputActions.InGame;
        uIActions = inputActions.UI;

        GameLoopManager.Instance.GameStateChanged += OnGameStateChanged;
    }
    private void OnEnable()
    {
        uIActions.Escape.performed += OnEscape;
        uIActions.Menu.performed += OnMenuToggled;

        uIActions.Hotkey1.performed += OnHotkey1;
        uIActions.Hotkey2.performed += OnHotkey2;
        uIActions.Hotkey3.performed += OnHotkey3;
        uIActions.Hotkey4.performed += OnHotkey4;
        uIActions.Hotkey5.performed += OnHotkey5;
        uIActions.Hotkey6.performed += OnHotkey6;
    }
    private void OnDisable()
    {
        uIActions.Escape.performed -= OnEscape;
        uIActions.Menu.performed -= OnMenuToggled;

        uIActions.Hotkey1.performed -= OnHotkey1;
        uIActions.Hotkey2.performed -= OnHotkey2;
        uIActions.Hotkey3.performed -= OnHotkey3;
        uIActions.Hotkey4.performed -= OnHotkey4;
        uIActions.Hotkey5.performed -= OnHotkey5;
        uIActions.Hotkey6.performed -= OnHotkey6;
    }

    private void OnEscape(CallbackContext ctx)
    {
        bool priorityEscape = PriorityEscape.Invoke();

        if (priorityEscape)
        {
            return;
        }
        Escape.Invoke();
    }
    private void OnMenuToggled(CallbackContext context) { MenuToggled?.Invoke(); }

    private void OnHotkey1(CallbackContext context) { if (inGameActions.Shift.IsPressed()) { MenuHotkey1?.Invoke(); } else { Hotkey1?.Invoke(); } }
    private void OnHotkey2(CallbackContext context) { if (inGameActions.Shift.IsPressed()) { MenuHotkey2?.Invoke(); } else { Hotkey2?.Invoke(); } }
    private void OnHotkey3(CallbackContext context) { if (inGameActions.Shift.IsPressed()) { MenuHotkey3?.Invoke(); } else { Hotkey3?.Invoke(); } }
    private void OnHotkey4(CallbackContext context) { if (inGameActions.Shift.IsPressed()) { MenuHotkey4?.Invoke(); } else { Hotkey4?.Invoke(); } }
    private void OnHotkey5(CallbackContext context) { if (inGameActions.Shift.IsPressed()) { MenuHotkey5?.Invoke(); } else { Hotkey5?.Invoke(); } }
    private void OnHotkey6(CallbackContext context) { if (inGameActions.Shift.IsPressed()) { MenuHotkey6?.Invoke(); } else { Hotkey6?.Invoke(); } }

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
