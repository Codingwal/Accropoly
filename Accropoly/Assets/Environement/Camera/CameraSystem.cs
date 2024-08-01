using UnityEngine;
using Cinemachine;
using System;
using UnityEngine.Animations;

public class CameraSystem : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;
    private Vector3 followOffset;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    private float mapSize;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed;

    [Header("Zooming")]
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float followOffsetMinY;
    [SerializeField] private float followOffsetMaxY;
    [SerializeField] private float zoomLerpSpeed;

    [Header("Looking")]
    [SerializeField] private float lookSpeed;

    [Header("Sprinting")]
    [SerializeField] private float sprintSpeedMultiplier;
    private float currentSpeedMultiplier;

    [SerializeField] private InputManager inputManager;
    Controls.InGameActions inGameActions;

    private void Start()
    {
        followOffset = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
        inputManager = InputManager.Instance;
    }
    private void FixedUpdate()
    {
        inGameActions = inputManager.inGameActions;
        CheckIfSprinting();
        MoveCamera();
        RotateCamera();
        ZoomCamera();
        Look();
    }
    private void OnEnable()
    {
        GameLoopManager.Instance.InitWorld += InitCameraSystem;
        GameLoopManager.Instance.SaveWorld += SaveCameraSystem;
    }
    private void OnDisable()
    {
        GameLoopManager.Instance.InitWorld -= InitCameraSystem;
        GameLoopManager.Instance.SaveWorld -= SaveCameraSystem;
    }

    private void InitCameraSystem(World world)
    {
        transform.SetPositionAndRotation(new(world.cameraSystemPos.x, 0, world.cameraSystemPos.y), world.cameraSystemRotation);

        followOffset.y = world.followOffsetY;

        mapSize = world.map.GetLength(0) * MapHandler.Instance.tileSize;
    }
    private void SaveCameraSystem(ref World world)
    {
        world.cameraSystemPos = new(transform.position.x, transform.position.z);
        world.cameraSystemRotation = transform.rotation;
        world.followOffsetY = followOffset.y;
    }

    private void CheckIfSprinting()
    {
        bool isSprinting = inGameActions.CameraSprint.IsPressed();

        if (isSprinting)
        {
            currentSpeedMultiplier = sprintSpeedMultiplier;
        }
        else
        {
            currentSpeedMultiplier = 1;
        }
    }

    private void MoveCamera()
    {
        // Get the input
        Vector2 moveDirInput = inGameActions.CameraMovement.ReadValue<Vector2>();

        // Transform it so it depends on object direction
        Vector3 moveDir = transform.forward * moveDirInput.y + transform.right * moveDirInput.x;

        // Move the camera
        transform.position += currentSpeedMultiplier * moveSpeed * Time.deltaTime * moveDir;

        float maxDistance = mapSize / 2;
        transform.position = new(Mathf.Clamp(transform.position.x, -maxDistance, maxDistance), 0, Mathf.Clamp(transform.position.z, -maxDistance, maxDistance));
    }
    private void RotateCamera()
    {
        // Get the input
        float rotateDirInput = inGameActions.CameraRotation.ReadValue<float>();

        // Rotate the camera
        transform.eulerAngles += new Vector3(0, rotateDirInput * rotationSpeed * currentSpeedMultiplier * Time.deltaTime, 0);
    }
    private void ZoomCamera()
    {
        Vector3 zoomDir = followOffset.normalized;

        float scrollInput = inGameActions.CameraScroll.ReadValue<float>();

        if (scrollInput > 0)
        {
            followOffset -= currentSpeedMultiplier * zoomSpeed * zoomDir;
        }
        if (scrollInput < 0)
        {
            followOffset += currentSpeedMultiplier * zoomSpeed * zoomDir;
        }

        if (followOffset.magnitude > followOffsetMaxY)
        {
            followOffset = followOffsetMaxY * zoomDir;
        }
        if (followOffset.magnitude < followOffsetMinY)
        {
            followOffset = followOffsetMinY * zoomDir;
        }

        cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset =
            Vector3.Lerp(cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset, followOffset, Time.deltaTime * zoomLerpSpeed);
    }
    private void Look()
    {
        // If the player isn't in CamerLook mode (middle mousebutton by default), set the cursorMode to free and return
        if (!inGameActions.CameraLook.IsPressed())
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector2 lookInput = inGameActions.MouseMove.ReadValue<Vector2>();

        Vector3 rotation = transform.eulerAngles;
        rotation += lookSpeed * Time.deltaTime * new Vector3(-lookInput.y, lookInput.x, 0);
        rotation.x = Mathf.Clamp(rotation.x, 285, 350); // Min = -75°, Max = -10° -> a total of 60°
        transform.eulerAngles = rotation;
    }
}
