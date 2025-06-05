using Unity.Collections;
using Unity.Entities;
using Components;

namespace Systems
{
    /// <summary>
    /// Handles taxes: The player gets money each morning
    /// totalTaxIncome = averageHappiness * totalPopulation * taxPerHappiness
    /// </summary>
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

            NativeArray<float> totalTaxIncome = new(1, Allocator.TempJob, NativeArrayOptions.ClearMemory);
            Entities.ForEach((in Person person) =>
            {
                totalTaxIncome[0] += person.happiness * config.taxPerHappiness;
            }).Schedule();

            // There probably is only one instance of both, this just simplifies the code
            Entities.ForEach((ref GameInfo info) =>
            {
                info.balance += totalTaxIncome[0];
            }).Schedule();
            Entities.ForEach((ref UIInfo info) =>
            {
                info.lastTaxIncome = totalTaxIncome[0];
            }).WithDisposeOnCompletion(totalTaxIncome).Schedule();
        }
    }
}