using System;
using Components;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public static Controls InputActions { get; private set; }
    public static Action<UIInputData> uiInput;
    private void Awake()
    {
        InputActions = new();
        DisableInputActions();
    }

    public static void EnableInputActions() { InputActions.Enable(); }
    public static void DisableInputActions() { InputActions.Disable(); }
    public static void EnableGameplayInputActions() { InputActions.InGame.Enable(); InputActions.UI.Enable(); }
    public static void DisableGameplayInputActions() { InputActions.InGame.Disable(); InputActions.UI.Disable(); }
    public static void EnableMenuInputActions() { InputActions.Menu.Enable(); }
    public static void DisableMenuInputActions() { InputActions.Menu.Disable(); }
}
