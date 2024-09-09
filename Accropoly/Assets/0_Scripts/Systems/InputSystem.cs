using Unity.Entities;
using UnityEngine;

using UIAction = UIInputData.Action;
using PlacementAction = PlacementInputData.Action;
using System;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class InputSystem : SystemBase
{
    public static InputSystem Instance { get; private set; }
    private Controls inputActions;
    private Entity inputDataHolder;

    public static Action escape;

    protected override void OnCreate()
    {
        if (Instance != null) Debug.LogError("More than one InputSystem instances");
        Instance = this;

        RequireForUpdate<RunGameTag>();

        inputActions = new();
        inputActions.Disable();

        inputDataHolder = EntityManager.CreateEntity(typeof(InputData), typeof(UIInputData), typeof(PlacementInputData));

        SystemAPI.SetComponentEnabled<UIInputData>(inputDataHolder, false);
        SystemAPI.SetComponentEnabled<PlacementInputData>(inputDataHolder, false);

        inputActions.InGame.Rotate.performed += (ctx) => OnPlacementAction(PlacementAction.Rotate);
        inputActions.InGame.Place.performed += (ctx) => OnPlacementAction(PlacementAction.Place);
        inputActions.InGame.Cancel.performed += (ctx) => OnPlacementAction(PlacementAction.Cancel);
        inputActions.UI.Escape.performed += (ctx) => OnUIAction(UIAction.Escape); ;
        inputActions.UI.Menu.performed += (ctx) => OnUIAction(UIAction.Menu); ;
        inputActions.UI.Clear.performed += (ctx) => OnUIAction(UIAction.Clear); ;
        inputActions.UI.Hotkey1.performed += (ctx) => OnUIAction(UIAction.Hotkey, 1); ;
        inputActions.UI.Hotkey2.performed += (ctx) => OnUIAction(UIAction.Hotkey, 2); ;
        inputActions.UI.Hotkey3.performed += (ctx) => OnUIAction(UIAction.Hotkey, 3); ;
        inputActions.UI.Hotkey4.performed += (ctx) => OnUIAction(UIAction.Hotkey, 4); ;
        inputActions.UI.Hotkey5.performed += (ctx) => OnUIAction(UIAction.Hotkey, 5); ;
        inputActions.UI.Hotkey6.performed += (ctx) => OnUIAction(UIAction.Hotkey, 6); ;
        inputActions.UI.Hotkey7.performed += (ctx) => OnUIAction(UIAction.Hotkey, 7); ;
    }
    protected override void OnUpdate()
    {
        // Update InputData
        var inGameActions = inputActions.InGame;
        SystemAPI.SetComponent(inputDataHolder, new InputData
        {
            camera = new CameraInputData
            {
                move = inGameActions.CameraMovement.ReadValue<Vector2>(),
                sprint = inGameActions.CameraSprint.IsPressed(),
                scroll = inGameActions.CameraScroll.ReadValue<float>(),
                rotate = inGameActions.CameraRotation.ReadValue<float>(),
                look = inGameActions.CameraLook.IsPressed(),
            },
            mouseMove = inGameActions.MouseMove.ReadValue<Vector2>(),
            shift = inGameActions.Shift.IsPressed(),
        });

        // Disable input event tags
        SystemAPI.SetComponentEnabled<PlacementInputData>(inputDataHolder, false);
        SystemAPI.SetComponentEnabled<UIInputData>(inputDataHolder, false);
    }
    public void EnableInputActions() { inputActions.Enable(); }
    public void DisableInputActions() { inputActions.Disable(); }
    private void OnPlacementAction(PlacementAction action)
    {
        SystemAPI.SetComponentEnabled<PlacementInputData>(inputDataHolder, true);
        SystemAPI.SetComponent(inputDataHolder, new PlacementInputData { action = action });
    }
    private void OnUIAction(UIAction action, int hotkey = -1)
    {
        if (action == UIAction.Escape) escape?.Invoke();

        SystemAPI.SetComponentEnabled<UIInputData>(inputDataHolder, true);
        SystemAPI.SetComponent(inputDataHolder, new UIInputData
        {
            action = action,
            hotkey = hotkey
        });
    }
}
