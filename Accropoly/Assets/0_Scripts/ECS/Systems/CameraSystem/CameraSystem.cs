using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Components;

namespace Systems
{
    /// <summary>
    /// Calculates camera data (position, zoom, rotation, ...) and updates the CameraTransform singleton
    /// Also updates the Camera GameObject using the singleton data
    /// </summary>
    public partial struct CameraSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Tags.RunGame>();
            state.RequireForUpdate<ConfigComponents.Camera>();
            // state.RequireForUpdate<InputData>();
        }
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<ConfigComponents.Camera>();
            var inputData = SystemAPI.GetSingleton<InputData>();
            var cameraTransform = SystemAPI.GetSingleton<CameraTransform>();

            // Apply transform values from last job
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

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            state.Dependency = new Job
            {
                ecb = ecb,
                config = config,
                inputData = inputData,
                deltaTime = Time.deltaTime,
                mapSize = WorldDataSystem.worldData.map.tiles.GetLength(0),
                transform = cameraTransform,
                transformHolder = SystemAPI.GetSingletonEntity<CameraTransform>(),
            }.Schedule(state.Dependency);
        }

        [BurstCompile]
        public partial struct Job : IJob
        {
            public EntityCommandBuffer ecb;
            public ConfigComponents.Camera config;
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

                float2 forwardDir = math.rotate(quaternion.EulerXYZ(new(0, transform.rot.y * math.PI / 180, 0)), new(0, 0, 1)).xz; // Calculate forward & right direction using the rotation
                float2 rightDir = math.rotate(quaternion.EulerXYZ(new(0, transform.rot.y * math.PI / 180, 0)), new(1, 0, 0)).xz;

                float2 moveDir = forwardDir * inputData.camera.move.y + rightDir * inputData.camera.move.x; // moveDir is dependent on rotation & input
                transform.pos.xz += currentSpeedMultiplier * config.moveSpeed * deltaTime * moveDir;

                transform.pos.xz = math.clamp(transform.pos.xz, -1, mapSize * 2 + 1); // Prevent flying away from the map


                // Rotate around the y-axis

                transform.rot.y += inputData.camera.rotate * config.rotationSpeed * currentSpeedMultiplier * deltaTime;

                // Zoom camera

                float scrollInput = inputData.camera.scroll;

                float zoomChange = 0f;
                if (scrollInput > 0) // Zoom speed is independent from scrolling speed
                    zoomChange = -currentSpeedMultiplier * config.zoomSpeed;
                else if (scrollInput < 0)
                    zoomChange = currentSpeedMultiplier * config.zoomSpeed;

                transform.camDist += zoomChange;
                transform.camDist = math.clamp(transform.camDist, config.minDistance, config.maxDistance); // prevents zooming too close / too far

                // Rotate freely around x- & y-axis

                if (!inputData.camera.look) // Free the cursor and skip the lookMode if the player isn't in lookMode (middle mousebutton by default)
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

                ecb.SetComponent(transformHolder, transform);
            }
        }
    }
}