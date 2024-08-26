using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct WorldDataSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        Debug.Log("Loading WorldData");

        WorldData data = SaveSystem.Instance.GetWorldData();

        Entity worldDataHolder = state.EntityManager.CreateSingleton<WorldData>();
        SystemAPI.SetComponent(worldDataHolder, data);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        Entity worldDataHolder = SystemAPI.GetSingletonEntity<WorldData>();
        WorldData worldData = SystemAPI.GetComponent<WorldData>(worldDataHolder);

        SaveSystem.Instance.SaveWorldData(worldData);

        worldData.population.Dispose();
        worldData.map.tiles.Dispose();

        state.EntityManager.DestroyEntity(worldDataHolder);
    }
}
