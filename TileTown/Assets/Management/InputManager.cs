using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    public Controls inputActions;
    public Controls.InGameActions inGameActions;
    private void Awake() {
        inputActions = new Controls();
        inGameActions = inputActions.InGame;
    }
    private void OnEnable() {
        inGameActions.Enable();
    }
    private void OnDisable() {
        inGameActions.Disable();
    }
}
