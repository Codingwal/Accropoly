using Unity.Entities;
using UnityEngine;

public partial class ElectricityPollutionSystem : SystemBase
{
    private uint frame;
    protected override void OnCreate()
    {
        RequireForUpdate<Polluter>();
        RequireForUpdate<ElectricityProducer>();
    }
    protected override void OnUpdate()
    {
        // Only run this function every 50 frames
        frame++;
        if (frame % 50 != 2) return;

        float totalPollution = 0;
        Entities.WithAll<ActiveTileTag, ElectricityProducer>().ForEach((in Polluter polluter) =>
        {
            totalPollution += polluter.pollution;
        }).Run();

        // Debug.Log($"Total electricity production pollution: {totalPollution}");
    }
}
