using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

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
    private Entity transformHolder;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RunGameTag>();
        state.RequireForUpdate<CameraConfig>();
        state.RequireForUpdate<InputData>();

        transformHolder = state.EntityManager.CreateSingleton<CameraTransform>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<CameraConfig>();
        var inputData = SystemAPI.GetSingleton<InputData>();
        var cameraTransform = SystemAPI.GetSingleton<CameraTransform>();

        Transform transform = Camera.main.transform;
        (transform.position, transform.rotation) = cameraTransform.GetCameraTransform();
        if (cameraTransform.cursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        new Job
        {
            config = config,
            inputData = inputData,
            deltaTime = Time.deltaTime,
            mapSize = WorldDataManager.worldData.map.tiles.GetLength(0),
            transform = cameraTransform,
            transformHolder = transformHolder,
        }.Schedule();
    }

    [BurstCompile]
    public partial struct Job : IJob
    {
        public CameraConfig config;
        public InputData inputData;
        public float deltaTime;
        public int mapSize;
        public CameraTransform transform;
        public Entity transformHolder;

        public void Execute()
        {
            // Set the currSpeedMultiplier to the sprintSpeed if the sprintButton is pressed
            float currentSpeedMultiplier = inputData.camera.sprint ? config.sprintSpeedMultiplier : 1;

            // Move

            float3 forwardDir = math.rotate(quaternion.EulerXYZ(transform.rot), new(0, 0, 1)); // Calculate forward & right direction using the rotation
            float3 rightDir = math.rotate(quaternion.EulerXYZ(transform.rot), new(1, 0, 0));

            float3 moveDir = forwardDir * inputData.camera.move.y + rightDir * inputData.camera.move.x; // moveDir is dependent on rotation & input
            transform.pos += currentSpeedMultiplier * config.moveSpeed * deltaTime * moveDir;

            transform.pos.xz = math.clamp(transform.pos.xz, new(-mapSize / 2), new(mapSize / 2)); // Prevent flying away from the map


            // Rotate around the y-axis

            transform.rot.y += inputData.camera.rotate * config.rotationSpeed * currentSpeedMultiplier * deltaTime;

            // Zoom camera

            float scrollInput = inputData.camera.scroll;

            float zoomChange = 0f;
            if (scrollInput > 0) // Zoom speed is independent from scrolling speed
                zoomChange = currentSpeedMultiplier * config.zoomSpeed;
            else if (scrollInput < 0)
                zoomChange = -currentSpeedMultiplier * config.zoomSpeed;

            transform.camDist += zoomChange;
            transform.camDist = math.clamp(transform.camDist, -config.maxDistance, -config.minDistance); // prevents zooming too close / too far

            // Rotate freely around x- & y-axis

            if (!inputData.camera.look) // Free the cursor and skip the lookCode if the player isn't in lookMode (middle mousebutton by default)
            {
                transform.cursorLocked = false;
            }
            else
            {
                transform.cursorLocked = true;

                float2 lookInput = inputData.mouseMove;

                transform.rot.xy += config.lookSpeed * deltaTime * new float2(-lookInput.y, lookInput.x);

                if (transform.rot.x > 180f)
                    transform.rot.x -= 360f;
                transform.rot.x = math.clamp(transform.rot.x, config.minAngle, config.maxAngle);
            }

            World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(transformHolder, transform);
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