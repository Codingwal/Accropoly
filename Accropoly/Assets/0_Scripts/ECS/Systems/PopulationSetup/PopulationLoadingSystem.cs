using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(CreationSystemGroup))]
public partial struct PopulationLoadingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PrefabEntity>();
        state.RequireForUpdate<LoadGameTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity prefab = SystemAPI.GetSingleton<PrefabEntity>();

        WorldData worldData = WorldDataSystem.worldData;
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        List<Person> population = worldData.population;
        foreach (Person person in population)
        {
            Entity entity = ecb.Instantiate(prefab); // Entity needs to be created on main thread so that a valid value is stored in the buffer

            float3 pos = new();
            foreach (var (component, enabled) in person.components)
            {
                void AddComponent<T>() where T : unmanaged, IComponentData
                {
                    ecb.AddComponent<T>(entity, (T)component);
                    if (component is IEnableableComponent)
                        ecb.SetComponentEnabled(entity, typeof(T), enabled);
                }

                Type type = component.GetType();
                if (type == typeof(PosComponent)) pos = ((PosComponent)component).pos;
                else if (type == typeof(PersonComponent)) AddComponent<PersonComponent>();
                else Debug.LogError($"Unexpected type {type.Name}");
            }
            ecb.SetComponent(entity, LocalTransform.FromPositionRotationScale(pos, quaternion.identity, 0.1f));
        }
    }
}