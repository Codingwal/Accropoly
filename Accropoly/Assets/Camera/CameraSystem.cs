using UnityEngine;
using Cinemachine;

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
    CinemachineTransposer cinemachineTransposer;

    [Header("Looking")]
    [SerializeField] private float lookSpeed;
    [SerializeField] private float minAngle;
    [SerializeField] private float maxAngle;

    [Header("Sprinting")]
    [SerializeField] private float sprintSpeedMultiplier;
    private float currentSpeedMultiplier;

    private Controls.InGameActions inGameActions;
    private void Awake()
    {
        cinemachineTransposer = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        followOffset = cinemachineTransposer.m_FollowOffset;
        inGameActions = InputManager.inGameActions;
    }
    private void OnEnable()
    {
        WorldDataManager.onWorldDataLoaded += InitCameraSystem;
        WorldDataManager.onWorldDataSaving += SaveCameraSystem;
    }
    private void OnDisable()
    {
        WorldDataManager.onWorldDataLoaded -= InitCameraSystem;
        WorldDataManager.onWorldDataSaving -= SaveCameraSystem;
    }
    private void FixedUpdate()
    {
        CheckIfSprinting();
        MoveCamera();
        RotateCamera();
        ZoomCamera();
        Look();
    }

    private void InitCameraSystem(ref WorldData world)
    {
        Debug.Log("Initializing CameraSystem");

        transform.SetPositionAndRotation(new(world.cameraSystemPos.x, 0, world.cameraSystemPos.y), world.cameraSystemRotation);

        followOffset.y = world.followOffsetY;

        mapSize = world.map.tiles.GetLength(0) * 30;
        mapSize = 500;
    }
    private void SaveCameraSystem(ref WorldData world)
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

        // Prevent flying of the map
        float maxDistance = mapSize / 2;
        transform.position = new(Mathf.Clamp(transform.position.x, -maxDistance, maxDistance), 0, Mathf.Clamp(transform.position.z, -maxDistance, maxDistance));
    }
    private void RotateCamera()
    {
        float rotateDirInput = inGameActions.CameraRotation.ReadValue<float>();
        transform.eulerAngles += new Vector3(0, rotateDirInput * rotationSpeed * currentSpeedMultiplier * Time.deltaTime, 0);
    }
    private void ZoomCamera()
    {
        Vector3 zoomDir = followOffset.normalized;

        float scrollInput = inGameActions.CameraScroll.ReadValue<float>();

        // Change the followOffset independent of scroll speed, just the direction is important
        if (scrollInput > 0)
        {
            followOffset -= currentSpeedMultiplier * zoomSpeed * zoomDir;
        }
        else if (scrollInput < 0)
        {
            followOffset += currentSpeedMultiplier * zoomSpeed * zoomDir;
        }

        // Clamp the followOffset
        if (followOffset.magnitude > followOffsetMaxY)
        {
            followOffset = followOffsetMaxY * zoomDir;
        }
        else if (followOffset.magnitude < followOffsetMinY)
        {
            followOffset = followOffsetMinY * zoomDir;
        }

        cinemachineTransposer.m_FollowOffset = Vector3.Lerp(cinemachineTransposer.m_FollowOffset, followOffset, Time.deltaTime * zoomLerpSpeed);
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

        if (rotation.x > 180f)
            rotation.x -= 360f;
        rotation.x = Mathf.Clamp(rotation.x, minAngle, maxAngle);

        transform.eulerAngles = rotation;
    }
}
