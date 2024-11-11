using Unity.Entities;

public partial class UpdatePopulationStatisticsSystem : SystemBase
{
    private EntityQuery populationQuery;
    protected override void OnCreate()
    {
        populationQuery = GetEntityQuery(typeof(PersonComponent));
    }
    protected override void OnUpdate()
    {
        int populationSize = populationQuery.CalculateEntityCount();

        Entities.ForEach((ref UIInfo info) =>
        {
            info.populationSize = populationSize;
        }).Run();
    }
}
