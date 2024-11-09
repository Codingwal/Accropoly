using Unity.Entities;

public partial class SearchesSpaceComponentsInitialization : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<Worker>();
    }
    protected override void OnUpdate()
    {
        Entities.WithAll<NewPersonTag, Worker>().ForEach((Entity entity) =>
        {
            EntityManager.SetComponentData(entity, new Worker { employer = new(-1, -1) });
            EntityManager.AddComponent<SearchesSpaceTag>(entity);
        }).WithStructuralChanges().Run();

        if (SystemAPI.HasSingleton<LoadGameTag>())
        {
            Entities.ForEach((Entity entity, in Worker worker) =>
            {
                if (worker.employer.Equals(new(-1, -1)))
                    EntityManager.AddComponent(entity, typeof(SearchesSpaceTag));
            }).WithStructuralChanges().Run();
        }
    }
}
