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
            // var ecb = SystemAPI.GetSingleton<EndCreationECBSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            // Entity prefab = SystemAPI.GetSingleton<ConfigComponents.PrefabEntity>().personPrefab;

            // WorldData worldData = WorldDataSystem.worldData;

            // List<PersonData> populationData = worldData.population;
            // foreach (PersonData personData in populationData)
            // {
            //     Entity entity = ecb.Instantiate(prefab);

            //     float3 pos = new();
            //     foreach (var (component, enabled) in personData.components)
            //     {
            //         void AddComponent<T>() where T : unmanaged, IComponentData
            //         {
            //             ecb.AddComponent<T>(entity, (T)component);
            //             if (component is IEnableableComponent)
            //                 ecb.SetComponentEnabled(entity, typeof(T), enabled);
            //         }
            //         void AddBuffer<T>() where T : unmanaged, IBufferElementData
            //         {
            //             var buffer = ecb.AddBuffer<T>(entity);
            //             List<T> values = component as List<T>;
            //             foreach (var value in values)
            //                 buffer.Add(value);
            //         }

            //         Type type = component.GetType();
            //         if (type == typeof(PosComponent)) pos = ((PosComponent)component).pos;
            //         else if (type == typeof(Person)) AddComponent<Person>();
            //         else if (type == typeof(Worker)) AddComponent<Worker>();
            //         else if (type == typeof(Traveller)) AddComponent<Traveller>();
            //         else if (type == typeof(MovementInfo)) AddComponent<MovementInfo>();
            //         else if (type == typeof(Speed)) AddComponent<Speed>();
            //         else if (type == typeof(CurveFollower)) AddComponent<CurveFollower>();
            //         else if (type == typeof(List<PathElement>)) AddBuffer<PathElement>();
            //         else if (type == typeof(Travelling)) AddComponent<Travelling>();
            //         else if (type == typeof(WantsToTravel)) AddComponent<WantsToTravel>();
            //         else if (type == typeof(FreeTime)) AddComponent<FreeTime>();
            //         else if (type == typeof(Resting)) AddComponent<Resting>();
            //         else if (type == typeof(Working)) AddComponent<Working>();
            //         else Debug.LogError($"Unexpected type {type.Name}");
            //     }
            //     ecb.SetComponent(entity, LocalTransform.FromPositionRotationScale(pos, quaternion.identity, 0.1f));
            // }
        }
    }
}