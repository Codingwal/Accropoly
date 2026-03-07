using Unity.Entities;
using UnityEngine;
using Components;

using UIAction = Components.UIInputData.Action;
using PlacementAction = Components.PlacementInputData.Action;
using System;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

namespace Systems
{
    /// <summary>
    /// Manage input singletons (Update them using current input data)
    /// </summary>
    [UpdateInGroup(typeof(LateInitializationSystemGroup))]
    public partial class InputSystem : SystemBase
    {
        private Entity inputDataHolder;

        protected override void OnCreate()
        {
            inputDataHolder = EntityManager.CreateEntity(typeof(InputData), typeof(UIInputData), typeof(PlacementInputData));

            SystemAPI.SetComponentEnabled<UIInputData>(inputDataHolder, false);
            SystemAPI.SetComponentEnabled<PlacementInputData>(inputDataHolder, false);

            foreach (InputAction action in InputHandler.InputActions)
            {
                if (action == InputHandler.InputActions.InGame.Place)
                    action.canceled += OnInputAction;
                else
                    action.performed += OnInputAction;
            }

            EntityManager.CreateSingleton<UIInfo>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndLateInitializationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            Controls inputActions = InputHandler.InputActions;

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
                    look = inGameActions.CameraLook.IsPressed() || inGameActions.Shift.IsPressed(),
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
        protected override void OnDestroy()
        {
            foreach (InputAction action in InputHandler.InputActions)
            {
                if (action == InputHandler.InputActions.InGame.Place)
                    action.canceled -= OnInputAction;
                else
                    action.performed -= OnInputAction;
            }
        }

        private void OnInputAction(InputAction.CallbackContext ctx)
        {
            Controls inputActions = InputHandler.InputActions;

            // Placement actions
            if (ctx.action == inputActions.InGame.Place) OnPlacementAction(PlacementAction.Place);
            else if (ctx.action == inputActions.InGame.Rotate) OnPlacementAction(PlacementAction.Rotate);
            else if (ctx.action == inputActions.InGame.Cancel) OnPlacementAction(PlacementAction.Cancel);

            // Fullscreen hotkey
            else if (ctx.action == inputActions.UI.Fullscreen) ToggleFullscreen();

            // UI actions 
            else if (ctx.action == inputActions.Menu.Escape) OnUIAction(UIAction.Escape);
            else if (ctx.action == inputActions.UI.Menu) OnUIAction(UIAction.Menu);
            else if (ctx.action == inputActions.UI.Clear) OnUIAction(UIAction.Clear);
            else if (ctx.action == inputActions.UI.HideUI) OnUIAction(UIAction.HideUI);

            // Mouse movement
            else if (inputActions.InGame.Get().actions.Contains(ctx.action)) { }

            // Hotkey (1-9)
            else
            {
                string str = ctx.action.name;
                Debug.Assert(str.Contains("Hotkey"));
                string numStr = str[^1].ToString();
                int value = int.Parse(numStr);
                Debug.Assert(value > 0 && value < 10);
                OnUIAction(UIAction.Hotkey, value);
            }
        }

        private void OnPlacementAction(PlacementAction action)
        {
            SystemAPI.SetComponentEnabled<PlacementInputData>(inputDataHolder, true);
            SystemAPI.SetComponent(inputDataHolder, new PlacementInputData { action = action });
        }
        private void OnUIAction(UIAction action, int value = -1)
        {
            var inputData = new UIInputData
            {
                action = action,
                hotkey = value
            };

            InputHandler.uiInput?.Invoke(inputData);

            SystemAPI.SetComponentEnabled<UIInputData>(inputDataHolder, true);
            SystemAPI.SetComponent(inputDataHolder, inputData);
        }
        private void ToggleFullscreen()
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
    }
}