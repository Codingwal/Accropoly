using Unity.Entities;
using UnityEngine;

public partial class ElectricityRenewabilitySystem : SystemBase
{
    protected override void OnUpdate()
    {
        float totalPollution = 0;
        Entities.WithAll<ActiveTileTag, ElectricityProducer>().ForEach((in Polluter polluter) =>
        {
            totalPollution += polluter.pollution;
        }).Run();

        Debug.Log($"Total electricity production pollution: {totalPollution}");
    }
}
