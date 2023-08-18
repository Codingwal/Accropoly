using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    public Controls inputActions;
    public Controls.InGameActions inGameActions;
    protected override void SingletonAwake() {
        inputActions = new();
        inGameActions = inputActions.InGame;

        inGameActions.Enable();
    }
}
