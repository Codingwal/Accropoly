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
        state.Enabled = false;

        string mapName = FileHandler.GetWorldName();
        string dataString = FileHandler.ReadFile("Saves", mapName);
        WorldData data = ParseWorldData(dataString);

        Entity worldDataHolder = state.EntityManager.CreateEntity(typeof(MapData));
        SystemAPI.SetComponent(worldDataHolder, data.map);
    }
    private WorldData ParseWorldData(string dataString)
    {
        WorldData obj = new();
        return obj;
    }
}
