using System.Collections.Generic;
using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Tags;

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
            // Ignore all rendering and transform related components
            var prefab = SystemAPI.GetSingleton<ConfigComponents.PrefabEntity>();
            NativeArray<ComponentType> typesToIgnore = state.EntityManager.GetChunk(prefab.prefab).Archetype.GetComponentTypes();

            // Convert to HashSet for faster search
            HashSet<ComponentType> typesToIgnoreSet = new();
            foreach (var type in typesToIgnore)
                typesToIgnoreSet.Add(type);
            typesToIgnoreSet.Add(typeof(Unemployed));
            typesToIgnoreSet.Add(typeof(Homeless));
            typesToIgnore.Dispose();

            WorldDataSystem.worldData.population = new();
            foreach ((var _, Entity entity) in SystemAPI.Query<RefRO<Person>>().WithEntityAccess())
            {
                List<(IComponentData, bool)> components = new();
                NativeArray<ComponentType> componentTypes = state.EntityManager.GetChunk(entity).Archetype.GetComponentTypes();

                EntityManager entityManager = state.EntityManager;
                foreach (var componentType in componentTypes)
                {
                    if (typesToIgnoreSet.Contains(componentType)) continue;

                    void AddComponentData<T>() where T : unmanaged, IComponentData
                    {
                        // If the component is enableable, check if it is enabled. Else set it to true 
                        bool isEnabled = !componentType.IsEnableable || entityManager.IsComponentEnabled(entity, componentType);
                        components.Add((entityManager.GetComponentData<T>(entity), isEnabled));
                    }
                    void AddTag<T>() where T : unmanaged, IComponentData
                    {
                        bool isEnabled = !componentType.IsEnableable || entityManager.IsComponentEnabled(entity, componentType);
                        components.Add((new T(), isEnabled));
                    }

                    if (componentType == typeof(Person)) AddComponentData<Person>();
                    else if (componentType == typeof(Worker)) AddComponentData<Worker>();
                    else if (componentType == typeof(Traveller)) AddComponentData<Traveller>();
                    else if (componentType == typeof(Travelling)) AddTag<Travelling>();
                    else if (componentType == typeof(WantsToTravel)) AddTag<WantsToTravel>();
                    else Debug.LogWarning($"Component of type {componentType} will not be serialized but also isn't present in typesToIgnore");
                }
                componentTypes.Dispose();

                // Save the position in a otherwise unused component
                components.Add((new PosComponent { pos = entityManager.GetComponentData<LocalTransform>(entity).Position }, true));

                WorldDataSystem.worldData.population.Add(new() { components = components });
            }
        }
    }
}