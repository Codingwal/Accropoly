using Unity.Entities;

[UpdateInGroup(typeof(ComponentInitializationSystemGroup))]
public partial class PersonComponentInitializationSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<PersonComponent>();
    }
    protected override void OnUpdate()
    {
        var ecb = SystemAPI.GetSingleton<EndComponentInitializationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        if (SystemAPI.HasSingleton<LoadGameTag>())
        {
            Entities.ForEach((Entity entity, in PersonComponent person) =>
            {
                if (person.homeTile.Equals(new(-1, -1)))
                    ecb.AddComponent(entity, typeof(HomelessTag));
            }).Schedule();
        }
    }
}