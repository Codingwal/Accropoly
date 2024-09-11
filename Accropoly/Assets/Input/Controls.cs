//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.7.0
//     from Assets/Input/Controls.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @Controls: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @Controls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Controls"",
    ""maps"": [
        {
            ""name"": ""InGame"",
            ""id"": ""c9d2dcd3-4689-4458-95d5-3881eefa346e"",
            ""actions"": [
                {
                    ""name"": ""CameraMovement"",
                    ""type"": ""Value"",
                    ""id"": ""b9cd4200-2348-4832-87a9-7d202000a435"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""CameraSprint"",
                    ""type"": ""Button"",
                    ""id"": ""4165d590-83fc-42b5-b01f-8ed4529507da"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""CameraScroll"",
                    ""type"": ""Value"",
                    ""id"": ""650a702f-ebe0-4eda-853e-464cfc769ac5"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""CameraRotation"",
                    ""type"": ""Value"",
                    ""id"": ""6adf67f1-b80a-4893-9a3a-10767107b108"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Shift"",
                    ""type"": ""Button"",
                    ""id"": ""25ba70fa-f3ce-470b-974d-649c01c4e83d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Rotate"",
                    ""type"": ""Button"",
                    ""id"": ""80dbd5dd-709e-44a1-b950-139a3fe8c748"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Cancel"",
                    ""type"": ""Button"",
                    ""id"": ""4556e98d-309e-419a-9a14-c58ee6d08f3b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Place"",
                    ""type"": ""Button"",
                    ""id"": ""f52ac7ca-4a1e-42dd-8341-238b7f46fb36"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""CameraLook"",
                    ""type"": ""Button"",
                    ""id"": ""8c0ef223-696f-48b1-a5ca-b475481a6138"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""MouseMove"",
                    ""type"": ""Value"",
                    ""id"": ""d7c28d22-d4b4-4ef9-a03e-44d1c801f855"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": ""NormalizeVector2"",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WASD"",
                    ""id"": ""5e8e0e86-8435-423e-b3f9-f0101b610c55"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraMovement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""bbce0c02-166d-4cde-8959-95157daf22a6"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""3be6ec7a-656d-4937-9fc3-3d1ef56c5129"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""72d6219d-fa77-4634-a1d1-99e459c28ac9"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""2b4ce309-29aa-4378-8000-0b4117eb95a1"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""90fe2f26-f7d3-41b2-ad4c-4fb8d815d6ad"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Place"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a5ace0ed-870a-4042-87df-8949a0e6527d"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Cancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""64da14e7-e0f2-4964-8db8-3e8e1c183491"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Rotate"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""82d797a5-52f2-473a-bdd1-fcc956d774f4"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Shift"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""QE"",
                    ""id"": ""ab9b0d56-01f3-40a9-8dda-6837b559ea25"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraRotation"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""8d511c7b-7f2f-4c47-9bde-20c31e51c5fe"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraRotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""ad155284-bad5-456c-8c78-13187be79fee"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraRotation"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Mouse scroll"",
                    ""id"": ""56e6b35a-e48b-4a16-9311-a1b1766413b2"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraScroll"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""2f747451-1ee5-4627-8ccb-907592a674ff"",
                    ""path"": ""<Mouse>/scroll/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraScroll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""ff1c794e-0863-492c-becc-798432b3a61c"",
                    ""path"": ""<Mouse>/scroll/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraScroll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""6f8afda5-2070-49f7-bede-a86d410060df"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraSprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d43e0485-6ad2-4696-a5be-16c4bd473d0d"",
                    ""path"": ""<Mouse>/middleButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraLook"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""38787e0b-68f5-456d-8569-399f1af4f00e"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""UI"",
            ""id"": ""adb67511-4bb6-495e-8ef5-fea1bd906034"",
            ""actions"": [
                {
                    ""name"": ""Menu"",
                    ""type"": ""Button"",
                    ""id"": ""123fe9f2-c13b-4db8-953f-d1aa8c4d444e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Clear"",
                    ""type"": ""Button"",
                    ""id"": ""1d785eba-4278-42a9-858e-4b9f40fdd40c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Hotkey1"",
                    ""type"": ""Button"",
                    ""id"": ""6a9e8041-d132-4cf8-b436-22d18546849d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Hotkey2"",
                    ""type"": ""Button"",
                    ""id"": ""0ac1708e-6b48-4829-8050-d5ee7f650639"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Hotkey3"",
                    ""type"": ""Button"",
                    ""id"": ""de429163-b4d4-4671-aec5-00a557042977"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Hotkey4"",
                    ""type"": ""Button"",
                    ""id"": ""8055c9fb-ded2-452c-a9ab-773942ed730b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Hotkey5"",
                    ""type"": ""Button"",
                    ""id"": ""43bb6ccf-2f7f-401f-b4db-ee6775f59356"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Hotkey6"",
                    ""type"": ""Button"",
                    ""id"": ""5884bfc6-11fa-4071-9e2d-4fd2f23463ec"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Hotkey7"",
                    ""type"": ""Button"",
                    ""id"": ""5780f40b-3259-4adf-8754-21e7df810aa8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""2b4edb86-417b-4b1e-a152-b404fda3ffae"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Hotkey1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""082256b8-a5c3-49ac-bb2c-d60dc0f38c02"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Hotkey2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""26ba4819-0e1b-4391-bccf-b45020c167b3"",
                    ""path"": ""<Keyboard>/3"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Hotkey3"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""20aa146d-2dcb-414e-9ec5-7ef485b0a390"",
                    ""path"": ""<Keyboard>/4"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Hotkey4"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a1e0d9ed-9854-4a46-abdd-d32cf5e5ccdb"",
                    ""path"": ""<Keyboard>/5"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Hotkey5"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""27a71fbf-6e2d-4280-a657-2728df3e368c"",
                    ""path"": ""<Keyboard>/6"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Hotkey6"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3249f896-051f-4876-bebe-737db0733c9d"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Menu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""492990fe-1d85-4cdd-9bab-0988c2593f2d"",
                    ""path"": ""<Keyboard>/7"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Hotkey7"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7ffd6e19-f9aa-43a2-a48c-a280518bbb0d"",
                    ""path"": ""<Keyboard>/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Clear"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Menu"",
            ""id"": ""81e66229-fbff-4142-ba6c-240b197b57df"",
            ""actions"": [
                {
                    ""name"": ""Escape"",
                    ""type"": ""Button"",
                    ""id"": ""52d71861-3c54-4c47-83c8-cc75059aa9b1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""ade32847-0281-4f98-97d7-bfb584cfab4c"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Escape"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // InGame
        m_InGame = asset.FindActionMap("InGame", throwIfNotFound: true);
        m_InGame_CameraMovement = m_InGame.FindAction("CameraMovement", throwIfNotFound: true);
        m_InGame_CameraSprint = m_InGame.FindAction("CameraSprint", throwIfNotFound: true);
        m_InGame_CameraScroll = m_InGame.FindAction("CameraScroll", throwIfNotFound: true);
        m_InGame_CameraRotation = m_InGame.FindAction("CameraRotation", throwIfNotFound: true);
        m_InGame_Shift = m_InGame.FindAction("Shift", throwIfNotFound: true);
        m_InGame_Rotate = m_InGame.FindAction("Rotate", throwIfNotFound: true);
        m_InGame_Cancel = m_InGame.FindAction("Cancel", throwIfNotFound: true);
        m_InGame_Place = m_InGame.FindAction("Place", throwIfNotFound: true);
        m_InGame_CameraLook = m_InGame.FindAction("CameraLook", throwIfNotFound: true);
        m_InGame_MouseMove = m_InGame.FindAction("MouseMove", throwIfNotFound: true);
        // UI
        m_UI = asset.FindActionMap("UI", throwIfNotFound: true);
        m_UI_Menu = m_UI.FindAction("Menu", throwIfNotFound: true);
        m_UI_Clear = m_UI.FindAction("Clear", throwIfNotFound: true);
        m_UI_Hotkey1 = m_UI.FindAction("Hotkey1", throwIfNotFound: true);
        m_UI_Hotkey2 = m_UI.FindAction("Hotkey2", throwIfNotFound: true);
        m_UI_Hotkey3 = m_UI.FindAction("Hotkey3", throwIfNotFound: true);
        m_UI_Hotkey4 = m_UI.FindAction("Hotkey4", throwIfNotFound: true);
        m_UI_Hotkey5 = m_UI.FindAction("Hotkey5", throwIfNotFound: true);
        m_UI_Hotkey6 = m_UI.FindAction("Hotkey6", throwIfNotFound: true);
        m_UI_Hotkey7 = m_UI.FindAction("Hotkey7", throwIfNotFound: true);
        // Menu
        m_Menu = asset.FindActionMap("Menu", throwIfNotFound: true);
        m_Menu_Escape = m_Menu.FindAction("Escape", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // InGame
    private readonly InputActionMap m_InGame;
    private List<IInGameActions> m_InGameActionsCallbackInterfaces = new List<IInGameActions>();
    private readonly InputAction m_InGame_CameraMovement;
    private readonly InputAction m_InGame_CameraSprint;
    private readonly InputAction m_InGame_CameraScroll;
    private readonly InputAction m_InGame_CameraRotation;
    private readonly InputAction m_InGame_Shift;
    private readonly InputAction m_InGame_Rotate;
    private readonly InputAction m_InGame_Cancel;
    private readonly InputAction m_InGame_Place;
    private readonly InputAction m_InGame_CameraLook;
    private readonly InputAction m_InGame_MouseMove;
    public struct InGameActions
    {
        private @Controls m_Wrapper;
        public InGameActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @CameraMovement => m_Wrapper.m_InGame_CameraMovement;
        public InputAction @CameraSprint => m_Wrapper.m_InGame_CameraSprint;
        public InputAction @CameraScroll => m_Wrapper.m_InGame_CameraScroll;
        public InputAction @CameraRotation => m_Wrapper.m_InGame_CameraRotation;
        public InputAction @Shift => m_Wrapper.m_InGame_Shift;
        public InputAction @Rotate => m_Wrapper.m_InGame_Rotate;
        public InputAction @Cancel => m_Wrapper.m_InGame_Cancel;
        public InputAction @Place => m_Wrapper.m_InGame_Place;
        public InputAction @CameraLook => m_Wrapper.m_InGame_CameraLook;
        public InputAction @MouseMove => m_Wrapper.m_InGame_MouseMove;
        public InputActionMap Get() { return m_Wrapper.m_InGame; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(InGameActions set) { return set.Get(); }
        public void AddCallbacks(IInGameActions instance)
        {
            if (instance == null || m_Wrapper.m_InGameActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_InGameActionsCallbackInterfaces.Add(instance);
            @CameraMovement.started += instance.OnCameraMovement;
            @CameraMovement.performed += instance.OnCameraMovement;
            @CameraMovement.canceled += instance.OnCameraMovement;
            @CameraSprint.started += instance.OnCameraSprint;
            @CameraSprint.performed += instance.OnCameraSprint;
            @CameraSprint.canceled += instance.OnCameraSprint;
            @CameraScroll.started += instance.OnCameraScroll;
            @CameraScroll.performed += instance.OnCameraScroll;
            @CameraScroll.canceled += instance.OnCameraScroll;
            @CameraRotation.started += instance.OnCameraRotation;
            @CameraRotation.performed += instance.OnCameraRotation;
            @CameraRotation.canceled += instance.OnCameraRotation;
            @Shift.started += instance.OnShift;
            @Shift.performed += instance.OnShift;
            @Shift.canceled += instance.OnShift;
            @Rotate.started += instance.OnRotate;
            @Rotate.performed += instance.OnRotate;
            @Rotate.canceled += instance.OnRotate;
            @Cancel.started += instance.OnCancel;
            @Cancel.performed += instance.OnCancel;
            @Cancel.canceled += instance.OnCancel;
            @Place.started += instance.OnPlace;
            @Place.performed += instance.OnPlace;
            @Place.canceled += instance.OnPlace;
            @CameraLook.started += instance.OnCameraLook;
            @CameraLook.performed += instance.OnCameraLook;
            @CameraLook.canceled += instance.OnCameraLook;
            @MouseMove.started += instance.OnMouseMove;
            @MouseMove.performed += instance.OnMouseMove;
            @MouseMove.canceled += instance.OnMouseMove;
        }

        private void UnregisterCallbacks(IInGameActions instance)
        {
            @CameraMovement.started -= instance.OnCameraMovement;
            @CameraMovement.performed -= instance.OnCameraMovement;
            @CameraMovement.canceled -= instance.OnCameraMovement;
            @CameraSprint.started -= instance.OnCameraSprint;
            @CameraSprint.performed -= instance.OnCameraSprint;
            @CameraSprint.canceled -= instance.OnCameraSprint;
            @CameraScroll.started -= instance.OnCameraScroll;
            @CameraScroll.performed -= instance.OnCameraScroll;
            @CameraScroll.canceled -= instance.OnCameraScroll;
            @CameraRotation.started -= instance.OnCameraRotation;
            @CameraRotation.performed -= instance.OnCameraRotation;
            @CameraRotation.canceled -= instance.OnCameraRotation;
            @Shift.started -= instance.OnShift;
            @Shift.performed -= instance.OnShift;
            @Shift.canceled -= instance.OnShift;
            @Rotate.started -= instance.OnRotate;
            @Rotate.performed -= instance.OnRotate;
            @Rotate.canceled -= instance.OnRotate;
            @Cancel.started -= instance.OnCancel;
            @Cancel.performed -= instance.OnCancel;
            @Cancel.canceled -= instance.OnCancel;
            @Place.started -= instance.OnPlace;
            @Place.performed -= instance.OnPlace;
            @Place.canceled -= instance.OnPlace;
            @CameraLook.started -= instance.OnCameraLook;
            @CameraLook.performed -= instance.OnCameraLook;
            @CameraLook.canceled -= instance.OnCameraLook;
            @MouseMove.started -= instance.OnMouseMove;
            @MouseMove.performed -= instance.OnMouseMove;
            @MouseMove.canceled -= instance.OnMouseMove;
        }

        public void RemoveCallbacks(IInGameActions instance)
        {
            if (m_Wrapper.m_InGameActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IInGameActions instance)
        {
            foreach (var item in m_Wrapper.m_InGameActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_InGameActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public InGameActions @InGame => new InGameActions(this);

    // UI
    private readonly InputActionMap m_UI;
    private List<IUIActions> m_UIActionsCallbackInterfaces = new List<IUIActions>();
    private readonly InputAction m_UI_Menu;
    private readonly InputAction m_UI_Clear;
    private readonly InputAction m_UI_Hotkey1;
    private readonly InputAction m_UI_Hotkey2;
    private readonly InputAction m_UI_Hotkey3;
    private readonly InputAction m_UI_Hotkey4;
    private readonly InputAction m_UI_Hotkey5;
    private readonly InputAction m_UI_Hotkey6;
    private readonly InputAction m_UI_Hotkey7;
    public struct UIActions
    {
        private @Controls m_Wrapper;
        public UIActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Menu => m_Wrapper.m_UI_Menu;
        public InputAction @Clear => m_Wrapper.m_UI_Clear;
        public InputAction @Hotkey1 => m_Wrapper.m_UI_Hotkey1;
        public InputAction @Hotkey2 => m_Wrapper.m_UI_Hotkey2;
        public InputAction @Hotkey3 => m_Wrapper.m_UI_Hotkey3;
        public InputAction @Hotkey4 => m_Wrapper.m_UI_Hotkey4;
        public InputAction @Hotkey5 => m_Wrapper.m_UI_Hotkey5;
        public InputAction @Hotkey6 => m_Wrapper.m_UI_Hotkey6;
        public InputAction @Hotkey7 => m_Wrapper.m_UI_Hotkey7;
        public InputActionMap Get() { return m_Wrapper.m_UI; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(UIActions set) { return set.Get(); }
        public void AddCallbacks(IUIActions instance)
        {
            if (instance == null || m_Wrapper.m_UIActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_UIActionsCallbackInterfaces.Add(instance);
            @Menu.started += instance.OnMenu;
            @Menu.performed += instance.OnMenu;
            @Menu.canceled += instance.OnMenu;
            @Clear.started += instance.OnClear;
            @Clear.performed += instance.OnClear;
            @Clear.canceled += instance.OnClear;
            @Hotkey1.started += instance.OnHotkey1;
            @Hotkey1.performed += instance.OnHotkey1;
            @Hotkey1.canceled += instance.OnHotkey1;
            @Hotkey2.started += instance.OnHotkey2;
            @Hotkey2.performed += instance.OnHotkey2;
            @Hotkey2.canceled += instance.OnHotkey2;
            @Hotkey3.started += instance.OnHotkey3;
            @Hotkey3.performed += instance.OnHotkey3;
            @Hotkey3.canceled += instance.OnHotkey3;
            @Hotkey4.started += instance.OnHotkey4;
            @Hotkey4.performed += instance.OnHotkey4;
            @Hotkey4.canceled += instance.OnHotkey4;
            @Hotkey5.started += instance.OnHotkey5;
            @Hotkey5.performed += instance.OnHotkey5;
            @Hotkey5.canceled += instance.OnHotkey5;
            @Hotkey6.started += instance.OnHotkey6;
            @Hotkey6.performed += instance.OnHotkey6;
            @Hotkey6.canceled += instance.OnHotkey6;
            @Hotkey7.started += instance.OnHotkey7;
            @Hotkey7.performed += instance.OnHotkey7;
            @Hotkey7.canceled += instance.OnHotkey7;
        }

        private void UnregisterCallbacks(IUIActions instance)
        {
            @Menu.started -= instance.OnMenu;
            @Menu.performed -= instance.OnMenu;
            @Menu.canceled -= instance.OnMenu;
            @Clear.started -= instance.OnClear;
            @Clear.performed -= instance.OnClear;
            @Clear.canceled -= instance.OnClear;
            @Hotkey1.started -= instance.OnHotkey1;
            @Hotkey1.performed -= instance.OnHotkey1;
            @Hotkey1.canceled -= instance.OnHotkey1;
            @Hotkey2.started -= instance.OnHotkey2;
            @Hotkey2.performed -= instance.OnHotkey2;
            @Hotkey2.canceled -= instance.OnHotkey2;
            @Hotkey3.started -= instance.OnHotkey3;
            @Hotkey3.performed -= instance.OnHotkey3;
            @Hotkey3.canceled -= instance.OnHotkey3;
            @Hotkey4.started -= instance.OnHotkey4;
            @Hotkey4.performed -= instance.OnHotkey4;
            @Hotkey4.canceled -= instance.OnHotkey4;
            @Hotkey5.started -= instance.OnHotkey5;
            @Hotkey5.performed -= instance.OnHotkey5;
            @Hotkey5.canceled -= instance.OnHotkey5;
            @Hotkey6.started -= instance.OnHotkey6;
            @Hotkey6.performed -= instance.OnHotkey6;
            @Hotkey6.canceled -= instance.OnHotkey6;
            @Hotkey7.started -= instance.OnHotkey7;
            @Hotkey7.performed -= instance.OnHotkey7;
            @Hotkey7.canceled -= instance.OnHotkey7;
        }

        public void RemoveCallbacks(IUIActions instance)
        {
            if (m_Wrapper.m_UIActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IUIActions instance)
        {
            foreach (var item in m_Wrapper.m_UIActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_UIActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public UIActions @UI => new UIActions(this);

    // Menu
    private readonly InputActionMap m_Menu;
    private List<IMenuActions> m_MenuActionsCallbackInterfaces = new List<IMenuActions>();
    private readonly InputAction m_Menu_Escape;
    public struct MenuActions
    {
        private @Controls m_Wrapper;
        public MenuActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Escape => m_Wrapper.m_Menu_Escape;
        public InputActionMap Get() { return m_Wrapper.m_Menu; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MenuActions set) { return set.Get(); }
        public void AddCallbacks(IMenuActions instance)
        {
            if (instance == null || m_Wrapper.m_MenuActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_MenuActionsCallbackInterfaces.Add(instance);
            @Escape.started += instance.OnEscape;
            @Escape.performed += instance.OnEscape;
            @Escape.canceled += instance.OnEscape;
        }

        private void UnregisterCallbacks(IMenuActions instance)
        {
            @Escape.started -= instance.OnEscape;
            @Escape.performed -= instance.OnEscape;
            @Escape.canceled -= instance.OnEscape;
        }

        public void RemoveCallbacks(IMenuActions instance)
        {
            if (m_Wrapper.m_MenuActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IMenuActions instance)
        {
            foreach (var item in m_Wrapper.m_MenuActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_MenuActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public MenuActions @Menu => new MenuActions(this);
    public interface IInGameActions
    {
        void OnCameraMovement(InputAction.CallbackContext context);
        void OnCameraSprint(InputAction.CallbackContext context);
        void OnCameraScroll(InputAction.CallbackContext context);
        void OnCameraRotation(InputAction.CallbackContext context);
        void OnShift(InputAction.CallbackContext context);
        void OnRotate(InputAction.CallbackContext context);
        void OnCancel(InputAction.CallbackContext context);
        void OnPlace(InputAction.CallbackContext context);
        void OnCameraLook(InputAction.CallbackContext context);
        void OnMouseMove(InputAction.CallbackContext context);
    }
    public interface IUIActions
    {
        void OnMenu(InputAction.CallbackContext context);
        void OnClear(InputAction.CallbackContext context);
        void OnHotkey1(InputAction.CallbackContext context);
        void OnHotkey2(InputAction.CallbackContext context);
        void OnHotkey3(InputAction.CallbackContext context);
        void OnHotkey4(InputAction.CallbackContext context);
        void OnHotkey5(InputAction.CallbackContext context);
        void OnHotkey6(InputAction.CallbackContext context);
        void OnHotkey7(InputAction.CallbackContext context);
    }
    public interface IMenuActions
    {
        void OnEscape(InputAction.CallbackContext context);
    }
}
