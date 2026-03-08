using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Components;
using Tags;
using Unity.Burst;

namespace Systems
{
    /// <summary>
    /// Calculate happiness for each person
    /// </summary>
    public partial class HappinessSystem : SystemBase
    {
        private int frame;

        protected override void OnUpdate()
        {
            // Only execute every 50 frames
            frame++;
            if (frame % 50 != 0) return;

            NativeReference<float> happinessSum = new(0, Allocator.TempJob);

            new CalculateHappinessJob
            {
                config = ConfigData.populationConfig.Data.happiness,
                hasElectricityLookup = GetComponentLookup<HasElectricity>(),
                workerLookup = GetComponentLookup<Worker>(),
                unemployedLookup = GetComponentLookup<Unemployed>(),
                buffer = TileGridUtility.GetEntityGrid(),
                happinessSum = happinessSum,
            }.Schedule();

            new UpdateHappinessInfoJob
            {
                happinessSum = happinessSum,
            }.Schedule();

            happinessSum.Dispose(Dependency);
        }

        [BurstCompile]
        private partial struct UpdateHappinessInfoJob : IJobEntity
        {
            [ReadOnly] public NativeReference<float> happinessSum;
            public void Execute(ref UIInfo info)
            {
                info.happinessSum = happinessSum.Value;
            }
        }

        [BurstCompile]
        private partial struct CalculateHappinessJob : IJobEntity
        {
            public PopulationConfig.Happiness config;
            [ReadOnly] public ComponentLookup<HasElectricity> hasElectricityLookup;
            [ReadOnly] public ComponentLookup<Worker> workerLookup;
            [ReadOnly] public ComponentLookup<Unemployed> unemployedLookup;
            public DynamicBuffer<EntityBufferElement> buffer;
            public NativeReference<float> happinessSum;
            public void Execute(Entity entity, ref Person person)
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
                        if (hasElectricityLookup.HasComponent(habitatEntity))
                        {
                            bool hasElectricity = hasElectricityLookup.IsComponentEnabled(habitatEntity);
                            person.happiness += hasElectricity ? config.hasElectricity : config.noElectricity;
                        }
                    }
                }

                // Work factors
                if (workerLookup.HasComponent(entity))
                {
                    bool employed = !unemployedLookup.HasComponent(entity);
                    person.happiness += employed ? config.employed : config.unemployed;
                }

                person.happiness = math.clamp(person.happiness, 0, 100);
                happinessSum.Value += person.happiness;
            }
        }
    }
}