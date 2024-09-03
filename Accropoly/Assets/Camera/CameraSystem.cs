using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

using UIAction = UIInputData.Action;

public partial struct CameraSystem : ISystem
{
    // private void OnEnable()
    // {
    // WorldDataManager.onWorldDataLoaded += InitCameraSystem;
    // WorldDataManager.onWorldDataSaving += SaveCameraSystem;
    // }
    // private void OnDisable()
    // {
    // WorldDataManager.onWorldDataLoaded -= InitCameraSystem;
    // WorldDataManager.onWorldDataSaving -= SaveCameraSystem;
    // }
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RunGameTag>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<CameraConfig>();

        var inputData = SystemAPI.GetSingleton<InputData>();

        // Get the input
        new Job
        {
            config = config,
            inputData = inputData,
            pos = Camera.main.transform.position,
            rotation = Camera.main.transform.rotation.eulerAngles,
        }.Schedule();
    }

    [BurstCompile]
    public partial struct Job : IJob
    {
        public CameraConfig config;
        public InputData inputData;
        public float deltaTime;
        public int mapSize;

        public float3 pos;
        public float3 rotation;
        public float3 camPos;

        readonly Vector3 Forward => math.rotate(quaternion.EulerXYZ(rotation), new(0, 0, 1));
        readonly Vector3 Right => math.rotate(quaternion.EulerXYZ(rotation), new(1, 0, 0));
        public void Execute()
        {
            // Set the currSpeedMultiplier to the sprintSpeed if the sprintButton is pressed
            float currentSpeedMultiplier = inputData.camera.sprint ? config.sprintSpeedMultiplier : 1;

            // Move
            float3 moveDir = Forward * inputData.camera.move.y + Right * inputData.camera.move.x; // moveDir is dependent on rotation & input
            pos += currentSpeedMultiplier * config.moveSpeed * deltaTime * moveDir;

            pos.xz = math.clamp(pos.xz, new(-mapSize / 2), new(mapSize / 2)); // Prevent flying away from the map


            // Rotate around the y-axis
            rotation.y += inputData.camera.rotate * config.rotationSpeed * currentSpeedMultiplier * deltaTime;

            // Zoom camera
            float scrollInput = inputData.camera.scroll;

            float zoomChange = 0f;
            if (scrollInput > 0) // Zoom speed is independent from scrolling speed
                zoomChange = currentSpeedMultiplier * config.zoomSpeed;
            else if (scrollInput < 0)
                zoomChange = -currentSpeedMultiplier * config.zoomSpeed;

            camPos.z += zoomChange;
            camPos.z = math.clamp(camPos.z, -config.maxDistance, -config.minDistance); // prevents zooming too close / too far

            // Rotate freely around x- & y-axis
            if (!inputData.camera.look) // Free the cursor and skip the lookCode if the player isn't in lookMode (middle mousebutton by default)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                float2 lookInput = inputData.mouseMove;

                rotation.xy += config.lookSpeed * Time.deltaTime * new float2(-lookInput.y, lookInput.x);

                if (rotation.x > 180f)
                    rotation.x -= 360f;
                rotation.x = math.clamp(rotation.x, config.minAngle, config.maxAngle);
            }

        }
    }

    // private void InitCameraSystem(ref WorldData world)
    // {
    //     Debug.Log("Initializing CameraSystem");

    //     transform.SetPositionAndRotation(new(world.cameraSystemPos.x, 0, world.cameraSystemPos.y), world.cameraSystemRotation);

    //     camera.transform.localPosition = new(0, 0, world.cameraDistance);

    //     mapSize = world.map.tiles.GetLength(0);
    // }
    // private void SaveCameraSystem(ref WorldData world)
    // {
    //     world.cameraSystemPos = new(transform.position.x, transform.position.z);
    //     world.cameraSystemRotation = transform.rotation;
    //     world.cameraDistance = camera.transform.localPosition.z;
    // }
}
