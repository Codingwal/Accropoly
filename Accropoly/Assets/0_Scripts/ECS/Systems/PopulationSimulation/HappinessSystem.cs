using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

public partial class HappinessSystem : SystemBase
{
    private int frame;
    protected override void OnCreate()
    {
        RequireForUpdate<RunGameTag>();
        RequireForUpdate<HappinessConfig>();
    }
    protected override void OnUpdate()
    {
        frame++;
        if (frame % 50 != 0) return;

        var buffer = SystemAPI.GetBuffer<EntityBufferElement>(SystemAPI.GetSingletonEntity<EntityGridHolder>());
        var config = SystemAPI.GetSingleton<HappinessConfig>();
        var hasElectricityTagLookup = GetComponentLookup<HasElectricityTag>();

        NativeArray<float> happinessSum = new NativeArray<float>(1, Allocator.TempJob, NativeArrayOptions.ClearMemory);
        Entities.ForEach((Entity entity, ref PersonComponent personComponent) =>
        {
            personComponent.happiness = config.defaultHappiness;

            // Habitat factors
            {
                int2 homeTile = personComponent.homeTile;
                if (homeTile.Equals(new(-1))) // (-1, -1) means missing (in this case homeless)
                    personComponent.happiness += config.homeless;
                else
                {
                    Entity habitatEntity = TileGridUtility.GetTile(homeTile, buffer);
                    bool hasElectricity = hasElectricityTagLookup.IsComponentEnabled(habitatEntity);
                    personComponent.happiness += hasElectricity ? config.hasElectricity : config.noElectricity;
                }
            }

            // Work factors
            if (SystemAPI.HasComponent<Worker>(entity))
            {
                bool employed = !SystemAPI.HasComponent<UnemployedTag>(entity);
                personComponent.happiness += employed ? config.employed : config.unemployed;
            }

            personComponent.happiness = math.clamp(personComponent.happiness, 0, 100);
            happinessSum[0] += personComponent.happiness;
        }).Schedule();

        Entities.ForEach((ref UIInfo info) =>
        {
            info.happinessSum = happinessSum[0];
        }).WithDisposeOnCompletion(happinessSum).Schedule();
    }
}