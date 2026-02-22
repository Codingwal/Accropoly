using Unity.Entities;
using Components;
using Tags;

namespace Systems
{
    /// <summary>
    /// Initialize "space components" (habitats, employers) 
    /// -> set free space to total space, add HasSpace tag
    /// Also re-adds HasSpace tag after world loading
    /// </summary>
    [UpdateInGroup(typeof(ComponentInitializationSystemGroup))]
    public partial class SpaceComponentsInitialization : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndComponentInitializationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            // Init new habitats / employers
            foreach (var (habitat, entity) in SystemAPI.Query<RefRW<Habitat>>().WithAll<NewTile>().WithEntityAccess())
            {
                habitat.ValueRW.freeSpace = habitat.ValueRO.totalSpace;
                ecb.AddComponent<HasSpace>(entity);
            }
            foreach (var (employer, entity) in SystemAPI.Query<RefRW<Employer>>().WithAll<NewTile>().WithEntityAccess())
            {
                employer.ValueRW.freeSpace = employer.ValueRO.totalSpace;
                ecb.AddComponent<HasSpace>(entity);
            }

            // Re-add tag after world loading
            if (SystemAPI.HasSingleton<LoadGame>())
            {
                foreach (var (habitat, entity) in SystemAPI.Query<RefRO<Habitat>>().WithEntityAccess())
                {
                    if (habitat.ValueRO.freeSpace > 0)
                        ecb.AddComponent<HasSpace>(entity);
                }
                foreach (var (employer, entity) in SystemAPI.Query<RefRO<Employer>>().WithEntityAccess())
                {
                    if (employer.ValueRO.freeSpace > 0)
                        ecb.AddComponent<HasSpace>(entity);
                }
            }
        }
    }
}