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
            RequireForUpdate<RunGame>();

            populationQuery = GetEntityQuery(typeof(Person));
            unemployedQuery = GetEntityQuery(typeof(Unemployed));
        }
        protected override void OnUpdate()
        {
            int populationSize = populationQuery.CalculateEntityCount();
            int unemployedCount = unemployedQuery.CalculateEntityCount();

            Entities.ForEach((ref UIInfo info) =>
            {
                info.populationSize = populationSize;
                info.unemployedCount = unemployedCount;
            }).Run();
        }
    }
}