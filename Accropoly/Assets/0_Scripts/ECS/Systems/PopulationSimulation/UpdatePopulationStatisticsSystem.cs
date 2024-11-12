using Unity.Entities;

public partial class UpdatePopulationStatisticsSystem : SystemBase
{
    private EntityQuery populationQuery;
    private EntityQuery unemployedQuery;
    protected override void OnCreate()
    {
        populationQuery = GetEntityQuery(typeof(PersonComponent));
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
