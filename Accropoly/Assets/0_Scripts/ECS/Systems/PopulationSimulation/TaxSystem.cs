using Unity.Collections;
using Unity.Entities;
using Components;

namespace Systems
{
    /// <summary>
    /// Handles taxes: The player gets money each morning
    /// totalTaxIncome = averageHappiness * totalPopulation * taxPerHappiness
    /// </summary>
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class TaxSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<Tags.NewDay>();
            RequireForUpdate<ConfigComponents.Taxes>();
        }
        protected override void OnUpdate()
        {
            var config = SystemAPI.GetSingleton<ConfigComponents.Taxes>();

            float totalTaxIncome = 0;
            foreach (var person in SystemAPI.Query<RefRO<Person>>())
            {
                totalTaxIncome += person.ValueRO.happiness * config.taxPerHappiness;
            }

            SystemAPI.GetSingletonRW<GameInfo>().ValueRW.balance += totalTaxIncome;
            SystemAPI.GetSingletonRW<UIInfo>().ValueRW.lastTaxIncome = totalTaxIncome;
        }
    }
}