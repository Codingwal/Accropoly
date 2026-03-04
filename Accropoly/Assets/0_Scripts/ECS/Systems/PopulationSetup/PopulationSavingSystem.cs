using System.Collections.Generic;
using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Tags;
using System.IO;

namespace Systems
{
    /// <summary>
    /// Save population data to save file
    /// </summary>
    [UpdateInGroup(typeof(CreationSystemGroup))]
    public partial struct PopulationSavingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SaveGame>();
        }
        public void OnUpdate(ref SystemState state)
        {
            //     // Ignore all rendering and transform related components
            //     var prefab = SystemAPI.GetSingleton<ConfigComponents.PrefabEntity>();
            //     NativeArray<ComponentType> typesToIgnore = state.EntityManager.GetChunk(prefab.personPrefab).Archetype.GetComponentTypes();

            //     // Convert to HashSet for faster search
            //     HashSet<ComponentType> typesToIgnoreSet = new();
            //     foreach (var type in typesToIgnore)
            //         typesToIgnoreSet.Add(type);
            //     typesToIgnoreSet.Add(typeof(Unemployed));
            //     typesToIgnoreSet.Add(typeof(Homeless));
            //     typesToIgnore.Dispose();

            //     WorldDataSystem.worldData.population = new();
            //     foreach ((var _, Entity entity) in SystemAPI.Query<RefRO<Person>>().WithEntityAccess())
            //     {
            //         List<(object, bool)> components = new();
            //         NativeArray<ComponentType> componentTypes = state.EntityManager.GetChunk(entity).Archetype.GetComponentTypes();

            //         EntityManager entityManager = state.EntityManager;
            //         foreach (var componentType in componentTypes)
            //         {
            //             if (typesToIgnoreSet.Contains(componentType)) continue;

            //             void AddComponentData<T>() where T : unmanaged, IComponentData
            //             {
            //                 // If the component is enableable, check if it is enabled. Else set it to true 
            //                 bool isEnabled = !componentType.IsEnableable || entityManager.IsComponentEnabled(entity, componentType);
            //                 components.Add((entityManager.GetComponentData<T>(entity), isEnabled));
            //             }
            //             void AddBuffer<T>() where T : unmanaged, IBufferElementData
            //             {
            //                 var buffer = entityManager.GetBuffer<T>(entity, isReadOnly: true);
            //                 List<T> values = new();
            //                 foreach (var value in buffer)
            //                     values.Add(value);
            //                 components.Add((values, true));
            //             }
            //             void AddTag<T>() where T : unmanaged, IComponentData
            //             {
            //                 bool isEnabled = !componentType.IsEnableable || entityManager.IsComponentEnabled(entity, componentType);
            //                 components.Add((new T(), isEnabled));
            //             }

            //             if (componentType == typeof(Person)) AddComponentData<Person>();
            //             else if (componentType == typeof(Worker)) AddComponentData<Worker>();
            //             else if (componentType == typeof(Traveller)) AddComponentData<Traveller>();
            //             else if (componentType == typeof(MovementInfo)) AddComponentData<MovementInfo>();
            //             else if (componentType == typeof(Speed)) AddComponentData<Speed>();
            //             else if (componentType == typeof(CurveFollower)) AddComponentData<CurveFollower>();
            //             else if (componentType == ComponentType.ReadWrite<PathElement>()) AddBuffer<PathElement>();
            //             else if (componentType == typeof(Travelling)) AddTag<Travelling>();
            //             else if (componentType == typeof(WantsToTravel)) AddTag<WantsToTravel>();
            //             else if (componentType == typeof(FreeTime)) AddTag<FreeTime>();
            //             else if (componentType == typeof(Resting)) AddTag<Resting>();
            //             else if (componentType == typeof(Working)) AddTag<Working>();
            //             else Debug.LogWarning($"Component of type {componentType} will not be serialized but also isn't present in typesToIgnore");
            //         }
            //         componentTypes.Dispose();

            //         // Save the position in a otherwise unused component
            //         components.Add((new PosComponent { pos = entityManager.GetComponentData<LocalTransform>(entity).Position }, true));

            //         WorldDataSystem.worldData.population.Add(new() { components = components });
            //     }
        }
    }
}