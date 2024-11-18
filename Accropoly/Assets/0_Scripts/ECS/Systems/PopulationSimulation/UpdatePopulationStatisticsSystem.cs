using Unity.Entities;
using Components;

public partial class UpdatePopulationStatisticsSystem : SystemBase
{
    private EntityQuery populationQuery;
    private EntityQuery unemployedQuery;
    protected override void OnCreate()
    {
        RequireForUpdate<RunGameTag>();

        populationQuery = GetEntityQuery(typeof(Person));
        unemployedQuery = GetEntityQuery(typeof(UnemployedTag));
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
