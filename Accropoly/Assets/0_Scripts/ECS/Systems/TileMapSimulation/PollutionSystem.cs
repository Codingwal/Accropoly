using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Components;
using Tags;

namespace Systems
{
    public partial class PollutionSystem : SystemBase
    {
        private uint frame;
        protected override void OnCreate()
        {
            RequireForUpdate<RunGame>();
        }
        protected override void OnUpdate()
        {
            // Only run this function every 50 frames
            frame++;
            if (frame % 50 != 3) return;

            NativeArray<float> totalPollution = new(1, Allocator.TempJob, NativeArrayOptions.ClearMemory);
            NativeArray<float> electricityPollution = new(1, Allocator.TempJob, NativeArrayOptions.ClearMemory);

            Entities.WithAll<ActiveTile>().ForEach((in Polluter polluter) =>
            {
                totalPollution[0] += polluter.pollution;
            }).Schedule();
            Entities.WithAll<ActiveTile, ElectricityProducer>().ForEach((in Polluter polluter) =>
            {
                electricityPollution[0] += polluter.pollution;
            }).Schedule();

            Entities.ForEach((ref UIInfo info) =>
            {
                info.pollution = totalPollution[0];
                info.electricityPollution = electricityPollution[0];
            }).WithDisposeOnCompletion(totalPollution).WithDisposeOnCompletion(electricityPollution).Schedule();
        }
    }
}