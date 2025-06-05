using Unity.Entities;
using Components;
using Tags;

namespace Systems
{
    /// <summary>
    /// Clear deactivated space tiles -> reset freeSpace to totalSpace
    /// </summary>
    public partial class ClearDeactivatedHabitats : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<RunGame>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            // Clear deactivated habitats
            Entities.WithDisabled<ActiveTile>().ForEach((Entity entity, ref Habitat habitat) =>
            {
                habitat.freeSpace = habitat.totalSpace;
                ecb.AddComponent<HasSpace>(entity);
            }).Schedule();

            // Clear deactivated employers
            Entities.WithDisabled<ActiveTile>().ForEach((Entity entity, ref Employer employer) =>
            {
                employer.freeSpace = employer.totalSpace;
                ecb.AddComponent<HasSpace>(entity);
            }).Schedule();
        }
    }
}
