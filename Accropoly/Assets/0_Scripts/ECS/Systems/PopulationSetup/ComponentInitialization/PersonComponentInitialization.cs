using Unity.Entities;
using Components;
using Tags;

namespace Systems
{
    /// <summary>
    /// Re-add homeless tag to homeless people after world loading
    /// (The tag isn't stored in the save file to reduce size)
    /// </summary>
    [UpdateInGroup(typeof(ComponentInitializationSystemGroup))]
    public partial class PersonComponentInitialization : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<Person>();
            RequireForUpdate<LoadGame>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndComponentInitializationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            foreach (var (person, entity) in SystemAPI.Query<RefRO<Person>>().WithEntityAccess())
            {
                if (person.ValueRO.homeTile.Equals(new(-1, -1)))
                    ecb.AddComponent<Homeless>(entity);
            }
        }

    }
}