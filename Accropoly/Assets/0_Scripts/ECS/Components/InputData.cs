using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Components
{
    public struct InputData : IComponentData
    {
        public CameraInputData camera;
        public Vector2 mouseMove;
        public Vector2 mousePos;
        public bool shift;
    }
    public struct CameraInputData
    {
        public float2 move;
        public bool sprint;
        public float scroll;
        public float rotate;
        public bool look;
    }
    public struct UIInputData : IComponentData, IEnableableComponent
    {
        public enum Action
        {
            None,
            Ctrl,
            Clear,
            Menu,
            Escape,
            Hotkey
        }
        public Action action;
        public int hotkey;
    }
    public struct PlacementInputData : IComponentData, IEnableableComponent
    {
        public enum Action
        {
            None,
            Rotate,
            Place,
            Cancel
        }
        public Action action;
        public bool placementProcessRunning;
    }
}