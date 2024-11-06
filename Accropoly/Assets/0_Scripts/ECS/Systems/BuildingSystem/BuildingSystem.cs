using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

using PlacementAction = PlacementInputData.Action;
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct BuildingSystem : ISystem
{
    private EntityQuery placementInputDataQuery;
    private static Entity entity;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RunGameTag>();
        state.RequireForUpdate<TileToPlace>(); // Update only if there is a PlacementProcess running (The process is started by the menu)

        placementInputDataQuery = state.GetEntityQuery(typeof(PlacementInputData));
    }
    public void OnUpdate(ref SystemState state)
    {
        if (Time.timeScale == 0) return; // Return if the game is paused

        var localTransform = state.EntityManager.GetComponentData<LocalTransform>(entity);
        var tileToPlace = state.EntityManager.GetComponentData<TileToPlace>(entity);

        if (placementInputDataQuery.CalculateEntityCount() != 0) // If there is placementInput (can't use singleton functions bc of IEnableableComponent)
        {
            // The PlacementInputData is attached to the same entity as the singleton InputData
            var placementInputData = state.EntityManager.GetComponentData<PlacementInputData>(SystemAPI.GetSingletonEntity<InputData>());

            if (placementInputData.action == PlacementAction.Rotate)
            {
                tileToPlace.rotation = tileToPlace.rotation.Rotate(1);
                state.EntityManager.SetComponentData(entity, tileToPlace);
                state.EntityManager.SetComponentData(entity, localTransform.RotateY(math.radians(90)));
            }
            else if (placementInputData.action == PlacementAction.Cancel)
            {
                state.EntityManager.DestroyEntity(entity);
                return;
            }
            else if (placementInputData.action == PlacementAction.Place)
            {
                int2 pos = (int2)localTransform.Position.xz / 2;
                Entity oldTile = TileGridUtility.GetTile(pos);
                TileType newTileType = tileToPlace.tileType;

                // Set the archetype to the archetype of the newTileType
                var components = TilePlacingUtility.GetComponents(newTileType, pos, tileToPlace.rotation);
                components.Add((new NewTileTag(), true));

                EntityCommandBuffer ecb = new(Allocator.Temp);
                TilePlacingUtility.UpdateEntity(oldTile, components, ecb);
                ecb.Playback(state.EntityManager);
                ecb.Dispose();

                // Set the transform rotation according to the rotation of tileToPlace
                var transform = state.EntityManager.GetComponentData<LocalTransform>(oldTile);
                transform.Rotation = quaternion.EulerXYZ(0, tileToPlace.rotation.ToRadians(), 0);
                state.EntityManager.SetComponentData(oldTile, transform);

                MaterialsAndMeshesHolder.UpdateMeshAndMaterial(oldTile, newTileType); // Update the mesh according to the newTileType 
            }
        }
    }
    public static void StartPlacementProcess(TileType tileType)
    {
        EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (em.CreateEntityQuery(typeof(TileToPlace)).CalculateEntityCount() != 0) return; // Return if there already is a PlacementProcess

        var prefab = em.CreateEntityQuery(typeof(TilePrefab)).GetSingleton<TilePrefab>(); // Get the tilePrefab
        entity = em.Instantiate(prefab);
        em.AddComponentData(entity, new TileToPlace
        {
            tileType = tileType,
        });
        MaterialsAndMeshesHolder.UpdateMeshAndMaterial(entity, tileType); // Set mesh & material according to the specified tileType
    }
}