using System;
using System.Collections.Generic;
using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Tags;

namespace Systems
{
    /// <summary>
    /// Load population from save file
    /// </summary>
    [UpdateInGroup(typeof(CreationSystemGroup))]
    public partial struct PopulationLoadingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LoadGame>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndCreationECBSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            Entity prefab = SystemAPI.GetSingleton<ConfigComponents.PrefabEntity>();

            WorldData worldData = WorldDataSystem.worldData;

            List<PersonData> populationData = worldData.population;
            foreach (PersonData personData in populationData)
            {
                Entity entity = ecb.Instantiate(prefab); // Entity needs to be created on main thread so that a valid value is stored in the buffer

                float3 pos = new();
                foreach (var (component, enabled) in personData.components)
                {
                    void AddComponent<T>() where T : unmanaged, IComponentData
                    {
                        ecb.AddComponent<T>(entity, (T)component);
                        if (component is IEnableableComponent)
                            ecb.SetComponentEnabled(entity, typeof(T), enabled);
                    }

                    Type type = component.GetType();
                    if (type == typeof(PosComponent)) pos = ((PosComponent)component).pos;
                    else if (type == typeof(Person)) AddComponent<Person>();
                    else if (type == typeof(Worker)) AddComponent<Worker>();
                    else if (type == typeof(Traveller)) AddComponent<Traveller>();
                    else if (type == typeof(Travelling)) AddComponent<Travelling>();
                    else if (type == typeof(WantsToTravel)) AddComponent<WantsToTravel>();
                    else Debug.LogError($"Unexpected type {type.Name}");
                }
                ecb.SetComponent(entity, LocalTransform.FromPositionRotationScale(pos, quaternion.identity, 0.1f));
            }
        }
    }
}