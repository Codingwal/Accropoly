using Unity.Collections;
using Unity.Entities;
using Components;
using Tags;

namespace Systems
{
    /// <summary>
    /// Calculate totalPollution and pollution by electricity producers
    /// Also update UIInfo
    /// </summary>
    public partial class PollutionSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<RunGame>();
        }
        protected override void OnUpdate()
        {
            float totalPollution = 0;
            foreach (var polluter in SystemAPI.Query<RefRO<Polluter>>().WithAll<ActiveTile>())
            {
                totalPollution += polluter.ValueRO.pollution;
            }

            float electricityPollution = 0;
            foreach (var polluter in SystemAPI.Query<RefRO<Polluter>>().WithAll<ActiveTile>())
            {
                electricityPollution += polluter.ValueRO.pollution;
            }

            var uiInfo = SystemAPI.GetSingletonRW<UIInfo>();
            uiInfo.ValueRW.pollution = totalPollution;
            uiInfo.ValueRW.electricityPollution = electricityPollution;
        }
    }
}