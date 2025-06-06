using Systems;
using Unity.Entities;
using Components;
using Unity.Mathematics;

public partial struct SaveAndLoadCameraTransform : ISystem
{
    private EntityQuery loadGameQuery;
    private EntityQuery saveGameQuery;
    public void OnCreate(ref SystemState state)
    {
        loadGameQuery = state.GetEntityQuery(typeof(Tags.LoadGame));
        saveGameQuery = state.GetEntityQuery(typeof(Tags.SaveGame));
    }
    public void OnUpdate(ref SystemState state)
    {
        if (loadGameQuery.CalculateEntityCount() != 0)
        {
            var worldData = WorldDataSystem.worldData;
            CameraTransform cameraTransform = new CameraTransform
            {
                pos = new float3(worldData.cameraSystemPos.x, 0, worldData.cameraSystemPos.y),
                rot = worldData.cameraSystemRotation,
                camDist = worldData.cameraDistance,
                cursorLocked = false,
            };

            state.EntityManager.CreateSingleton(cameraTransform);
        }

        if (saveGameQuery.CalculateEntityCount() != 0)
        {
            var cameraTransform = SystemAPI.GetSingleton<CameraTransform>();

            WorldDataSystem.worldData.cameraSystemPos = cameraTransform.pos.xz;
            WorldDataSystem.worldData.cameraSystemRotation = cameraTransform.rot;
            WorldDataSystem.worldData.cameraDistance = cameraTransform.camDist;

            state.EntityManager.DestroyEntity(SystemAPI.GetSingletonEntity<CameraTransform>());
        }
    }
}
