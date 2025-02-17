using Unity.Entities;
using Components;
using Tags;

namespace Systems
{
    [UpdateInGroup(typeof(ComponentInitializationSystemGroup))]
    public partial class PersonComponentInitialization : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<Person>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndComponentInitializationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            if (SystemAPI.HasSingleton<LoadGame>())
            {
                Entities.ForEach((Entity entity, in Person person) =>
                {
                    if (person.homeTile.Equals(new(-1, -1)))
                        ecb.AddComponent<Homeless>(entity);
                }).Schedule();
            }
        }
    }
}