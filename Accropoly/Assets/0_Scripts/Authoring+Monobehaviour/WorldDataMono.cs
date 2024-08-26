using Unity.Entities;
using UnityEngine;

public class WorldDataMono : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Loading WorldData");

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        WorldData data = SaveSystem.Instance.GetWorldData();

        Entity worldDataHolder = entityManager.CreateSingleton<WorldData>();
        entityManager.SetComponentData(worldDataHolder, data);
    }

    void OnDisable()
    {
        Debug.Log("Saving WorldData");

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        Entity worldDataHolder = entityManager.CreateEntityQuery(typeof(WorldData)).GetSingletonEntity();
        WorldData worldData = entityManager.GetComponentData<WorldData>(worldDataHolder);

        SaveSystem.Instance.SaveWorldData(worldData);

        worldData.population.Dispose();
        worldData.map.tiles.Dispose();

        entityManager.DestroyEntity(worldDataHolder);
    }
}
