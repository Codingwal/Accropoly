using Unity.Collections;
using Unity.Entities;

public partial class TaxSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<NewDayTag>();
        RequireForUpdate<TaxesConfig>();
    }
    protected override void OnUpdate()
    {
        TaxesConfig config = SystemAPI.GetSingleton<TaxesConfig>();

        NativeArray<float> totalTaxIncome = new(1, Allocator.TempJob, NativeArrayOptions.ClearMemory);
        Entities.ForEach((in PersonComponent personComponent) =>
        {
            totalTaxIncome[0] += personComponent.happiness * config.taxPerHappiness;
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