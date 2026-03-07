using Unity.Entities;
using Components;
using Tags;

namespace Systems
{
    /// <summary>
    /// Update population related data in UIInfo (used for statistics display)
    /// </summary>
    public partial class UpdatePopulationStatistics : SystemBase
    {
        private EntityQuery populationQuery;
        private EntityQuery unemployedQuery;
        protected override void OnCreate()
        {
            populationQuery = GetEntityQuery(typeof(Person));
            unemployedQuery = GetEntityQuery(typeof(Unemployed));
        }
        protected override void OnUpdate()
        {
            int populationSize = populationQuery.CalculateEntityCount();
            int unemployedCount = unemployedQuery.CalculateEntityCount();

            var info = SystemAPI.GetSingletonRW<UIInfo>();
            info.ValueRW.populationSize = populationSize;
            info.ValueRW.unemployedCount = unemployedCount;
        }
    }
}