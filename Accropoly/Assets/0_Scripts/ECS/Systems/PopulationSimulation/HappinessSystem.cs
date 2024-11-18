using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using Components;

public partial class HappinessSystem : SystemBase
{
    private int frame;
    protected override void OnCreate()
    {
        RequireForUpdate<RunGameTag>();
        RequireForUpdate<ConfigComponents.Happiness>();
    }
    protected override void OnUpdate()
    {
        frame++;
        if (frame % 50 != 0) return;

        var buffer = SystemAPI.GetBuffer<EntityBufferElement>(SystemAPI.GetSingletonEntity<EntityGridHolder>());
        var config = SystemAPI.GetSingleton<ConfigComponents.Happiness>();
        var hasElectricityTagLookup = GetComponentLookup<HasElectricityTag>();

        NativeArray<float> happinessSum = new NativeArray<float>(1, Allocator.TempJob, NativeArrayOptions.ClearMemory);
        Entities.ForEach((Entity entity, ref Person person) =>
        {
            person.happiness = config.defaultHappiness;

            // Habitat factors
            {
                int2 homeTile = person.homeTile;
                if (homeTile.Equals(new(-1))) // (-1, -1) means missing (in this case homeless)
                    person.happiness += config.homeless;
                else
                {
                    Entity habitatEntity = TileGridUtility.GetTile(homeTile, buffer);
                    if (hasElectricityTagLookup.HasComponent(habitatEntity))
                    {
                        bool hasElectricity = hasElectricityTagLookup.IsComponentEnabled(habitatEntity);
                        person.happiness += hasElectricity ? config.hasElectricity : config.noElectricity;
                    }
                }
            }

            // Work factors
            if (SystemAPI.HasComponent<Worker>(entity))
            {
                bool employed = !SystemAPI.HasComponent<UnemployedTag>(entity);
                person.happiness += employed ? config.employed : config.unemployed;
            }

            person.happiness = math.clamp(person.happiness, 0, 100);
            happinessSum[0] += person.happiness;
        }).Schedule();

        Entities.ForEach((ref UIInfo info) =>
        {
            info.happinessSum = happinessSum[0];
        }).WithDisposeOnCompletion(happinessSum).Schedule();
    }
}