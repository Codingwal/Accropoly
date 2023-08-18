using UnityEngine;
using Cinemachine;

public class CameraSystem : MonoBehaviour
{
    [SerializeField] private float moveSpeed;

    [SerializeField] private float rotationSpeed;

    [SerializeField] private float zoomSpeed;
    [SerializeField] private float followOffsetMinY;
    [SerializeField] private float followOffsetMaxY;
    [SerializeField] private float zoomLerpSpeed;

    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;

    private Vector3 followOffset;

    private void Start()
    {
        followOffset = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
    }
    private void FixedUpdate()
    {
        MoveCamera();
        RotateCamera();
        ZoomCamera();
    }
    private void MoveCamera()
    {
        // Get the input
        Vector2 moveDirInput = InputManager.Instance.inGameActions.CameraMovement.ReadValue<Vector2>();

        // Transform it so it depends on object direction
        Vector3 moveDir = transform.forward * moveDirInput.y + transform.right * moveDirInput.x;

        // Move the camera
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }
    private void RotateCamera()
    {
        // Get the input
        float rotateDirInput = InputManager.Instance.inGameActions.CameraRotation.ReadValue<float>();

        // Rotate the camera
        transform.eulerAngles += new Vector3(0, rotateDirInput * rotationSpeed * Time.deltaTime, 0);
    }
    private void ZoomCamera()
    {
        // Get the input
        float scrollInput = InputManager.Instance.inGameActions.CameraScroll.ReadValue<float>();

        if (scrollInput > 0)
        {
            followOffset.y -=  zoomSpeed;
        }
        if (scrollInput < 0)
        {
            followOffset.y += zoomSpeed;
        }

        followOffset.y = Mathf.Clamp(followOffset.y, followOffsetMinY, followOffsetMaxY);

        cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset =
            Vector3.Lerp(cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset, followOffset, Time.deltaTime * zoomLerpSpeed);
    }
}
