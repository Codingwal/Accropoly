using UnityEngine;
using Cinemachine;
using System;

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

    [Header("Sprinting")]
    [SerializeField] private float sprintSpeedMultiplier;
    private float currentSpeedMultiplier;



    private void Awake()
    {

    }
    private void Start()
    {
        followOffset = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
    }
    private void FixedUpdate()
    {
        CheckIfSprinting();
        MoveCamera();
        RotateCamera();
        ZoomCamera();
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
        transform.SetPositionAndRotation(world.cameraSystemPos, world.cameraSystemRotation);

        followOffset.y = world.followOffsetY;

        mapSize = world.map.GetLength(0) * MapHandler.Instance.tileSize;
    }
    private void SaveCameraSystem(ref World world)
    {
        world.cameraSystemPos = transform.position;
        world.cameraSystemRotation = transform.rotation;
        world.followOffsetY = followOffset.y;
    }

    private void CheckIfSprinting()
    {
        bool isSprinting = InputManager.Instance.inGameActions.CameraSprint.IsPressed();

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
        Vector2 moveDirInput = InputManager.Instance.inGameActions.CameraMovement.ReadValue<Vector2>();

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
        float rotateDirInput = InputManager.Instance.inGameActions.CameraRotation.ReadValue<float>();

        // Rotate the camera
        transform.eulerAngles += new Vector3(0, rotateDirInput * rotationSpeed * currentSpeedMultiplier * Time.deltaTime, 0);
    }
    private void ZoomCamera()
    {
        // Get the input
        float scrollInput = InputManager.Instance.inGameActions.CameraScroll.ReadValue<float>();

        if (scrollInput > 0)
        {
            followOffset.y -= zoomSpeed * currentSpeedMultiplier;
        }
        if (scrollInput < 0)
        {
            followOffset.y += zoomSpeed * currentSpeedMultiplier;
        }

        followOffset.y = Mathf.Clamp(followOffset.y, followOffsetMinY, followOffsetMaxY);

        cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset =
            Vector3.Lerp(cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset, followOffset, Time.deltaTime * zoomLerpSpeed);
    }
}
