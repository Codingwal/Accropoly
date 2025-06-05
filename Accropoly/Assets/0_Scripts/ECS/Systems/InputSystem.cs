using Unity.Entities;
using UnityEngine;
using Components;

using UIAction = Components.UIInputData.Action;
using PlacementAction = Components.PlacementInputData.Action;
using System;

namespace Systems
{
    /// <summary>
    /// Manage input singletons (Update them using current input data)
    /// </summary>
    [UpdateInGroup(typeof(LateInitializationSystemGroup))]
    public partial class InputSystem : SystemBase
    {
        private Controls inputActions;
        private Entity inputDataHolder;

        public static Action<UIInputData> uiInput;

        protected override void OnCreate()
        {
            RequireForUpdate<Tags.RunGame>();

            inputActions = new();
            DisableInputActions();

            inputDataHolder = EntityManager.CreateEntity(typeof(InputData), typeof(UIInputData), typeof(PlacementInputData));

            SystemAPI.SetComponentEnabled<UIInputData>(inputDataHolder, false);
            SystemAPI.SetComponentEnabled<PlacementInputData>(inputDataHolder, false);

            inputActions.InGame.Rotate.performed += (ctx) => OnPlacementAction(PlacementAction.Rotate);
            inputActions.InGame.Place.canceled += (ctx) => OnPlacementAction(PlacementAction.Place);
            inputActions.InGame.Cancel.performed += (ctx) => OnPlacementAction(PlacementAction.Cancel);
            inputActions.UI.Menu.performed += (ctx) => OnUIAction(UIAction.Menu); ;
            inputActions.UI.Clear.performed += (ctx) => OnUIAction(UIAction.Clear); ;
            inputActions.UI.Hotkey1.performed += (ctx) => OnUIAction(UIAction.Hotkey, 1); ;
            inputActions.UI.Hotkey2.performed += (ctx) => OnUIAction(UIAction.Hotkey, 2); ;
            inputActions.UI.Hotkey3.performed += (ctx) => OnUIAction(UIAction.Hotkey, 3); ;
            inputActions.UI.Hotkey4.performed += (ctx) => OnUIAction(UIAction.Hotkey, 4); ;
            inputActions.UI.Hotkey5.performed += (ctx) => OnUIAction(UIAction.Hotkey, 5); ;
            inputActions.UI.Hotkey6.performed += (ctx) => OnUIAction(UIAction.Hotkey, 6); ;
            inputActions.UI.Hotkey7.performed += (ctx) => OnUIAction(UIAction.Hotkey, 7); ;
            inputActions.Menu.Escape.performed += (ctx) => OnUIAction(UIAction.Escape); ;
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndLateInitializationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            // Update InputData
            var inGameActions = inputActions.InGame;
            ecb.SetComponent(inputDataHolder, new InputData
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
                mousePos = inGameActions.MousePos.ReadValue<Vector2>(),
                shift = inGameActions.Shift.IsPressed(),
            });

            ecb.SetComponentEnabled<UIInputData>(inputDataHolder, false);

            bool placementProcessRunning = inputActions.InGame.Place.IsPressed();
            SystemAPI.SetComponent(inputDataHolder, new PlacementInputData { placementProcessRunning = placementProcessRunning, action = PlacementAction.None });
            SystemAPI.SetComponentEnabled<PlacementInputData>(inputDataHolder, placementProcessRunning);
        }
        public void EnableInputActions() { inputActions.Enable(); }
        public void DisableInputActions() { inputActions.Disable(); }
        public void EnableGameplayInputActions() { inputActions.InGame.Enable(); inputActions.UI.Enable(); }
        public void DisableGameplayInputActions() { inputActions.InGame.Disable(); inputActions.UI.Disable(); }
        public void EnableMenuInputActions() { inputActions.Menu.Enable(); }
        public void DisableMenuInputActions() { inputActions.Menu.Disable(); }

        private void OnPlacementAction(PlacementAction action)
        {
            SystemAPI.SetComponentEnabled<PlacementInputData>(inputDataHolder, true);
            SystemAPI.SetComponent(inputDataHolder, new PlacementInputData { action = action });
        }
        private void OnUIAction(UIAction action, int hotkey = -1)
        {
            var inputData = new UIInputData
            {
                action = action,
                hotkey = hotkey
            };

            uiInput?.Invoke(inputData);

            SystemAPI.SetComponentEnabled<UIInputData>(inputDataHolder, true);
            SystemAPI.SetComponent(inputDataHolder, inputData);
        }
    }
}