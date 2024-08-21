using System;
using static UnityEngine.InputSystem.InputAction;

public static class InputManager
{
    public static Controls inputActions;
    public static Controls.InGameActions inGameActions;
    public static Controls.UIActions uIActions;

    public static event Func<bool> PriorityEscape;
    public static event Action Escape;
    public static event Action MenuToggled;

    public static event Action Hotkey1;
    public static event Action Hotkey2;
    public static event Action Hotkey3;
    public static event Action Hotkey4;
    public static event Action Hotkey5;
    public static event Action Hotkey6;
    public static event Action Hotkey7;

    public static event Action MenuHotkey1;
    public static event Action MenuHotkey2;
    public static event Action MenuHotkey3;
    public static event Action MenuHotkey4;
    public static event Action MenuHotkey5;
    public static event Action MenuHotkey6;
    public static event Action MenuHotkey7;

    public static void Init()
    {
        inputActions = new Controls();
        inGameActions = inputActions.InGame;
        uIActions = inputActions.UI;

        GameLoopManager.Instance.GameStateChanged += OnGameStateChanged;

        uIActions.Escape.performed += OnEscape;
        uIActions.Menu.performed += OnMenuToggled;

        uIActions.Hotkey1.performed += OnHotkey1;
        uIActions.Hotkey2.performed += OnHotkey2;
        uIActions.Hotkey3.performed += OnHotkey3;
        uIActions.Hotkey4.performed += OnHotkey4;
        uIActions.Hotkey5.performed += OnHotkey5;
        uIActions.Hotkey6.performed += OnHotkey6;
        uIActions.Hotkey7.performed += OnHotkey7;
    }

    private static void OnEscape(CallbackContext ctx)
    {
        bool priorityEscape = PriorityEscape.Invoke();

        if (priorityEscape)
        {
            return;
        }
        Escape.Invoke();
    }
    private static void OnMenuToggled(CallbackContext context) { MenuToggled?.Invoke(); }

    private static void OnHotkey1(CallbackContext context) { if (inGameActions.Shift.IsPressed()) { MenuHotkey1?.Invoke(); } else { Hotkey1?.Invoke(); } }
    private static void OnHotkey2(CallbackContext context) { if (inGameActions.Shift.IsPressed()) { MenuHotkey2?.Invoke(); } else { Hotkey2?.Invoke(); } }
    private static void OnHotkey3(CallbackContext context) { if (inGameActions.Shift.IsPressed()) { MenuHotkey3?.Invoke(); } else { Hotkey3?.Invoke(); } }
    private static void OnHotkey4(CallbackContext context) { if (inGameActions.Shift.IsPressed()) { MenuHotkey4?.Invoke(); } else { Hotkey4?.Invoke(); } }
    private static void OnHotkey5(CallbackContext context) { if (inGameActions.Shift.IsPressed()) { MenuHotkey5?.Invoke(); } else { Hotkey5?.Invoke(); } }
    private static void OnHotkey6(CallbackContext context) { if (inGameActions.Shift.IsPressed()) { MenuHotkey6?.Invoke(); } else { Hotkey6?.Invoke(); } }
    private static void OnHotkey7(CallbackContext context) { if (inGameActions.Shift.IsPressed()) { MenuHotkey7?.Invoke(); } else { Hotkey7?.Invoke(); } }

    private static void OnGameStateChanged(GameState newGameState, GameState oldGameState)
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
