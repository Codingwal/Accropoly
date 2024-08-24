using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditorInternal;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct WorldDataSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Debug.Log("Loading WorldData");

        state.Enabled = false;

        // string mapName = FileHandler.GetWorldName();
        // string dataString = FileHandler.ReadFile("Saves", mapName);
        // WorldData data = ParseWorldData(dataString);

        Entity worldDataHolder = state.EntityManager.CreateSingleton<MapData>();
        // SystemAPI.SetComponent(worldDataHolder, data.map);
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        MapData mapData = SystemAPI.GetSingleton<MapData>();
        mapData.tiles.Dispose();
    }
}
