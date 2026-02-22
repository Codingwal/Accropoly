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
            new ClearDeactivatedHabitatsJob
            {
                ecb = ecb
            }.Schedule();

            // Clear deactivated employers
            new ClearDeactivatedEmployersJob
            {
                ecb = ecb
            }.Schedule();
        }
        [WithDisabled(typeof(ActiveTile))]
        private partial struct ClearDeactivatedHabitatsJob : IJobEntity
        {
            public EntityCommandBuffer ecb;

            public void Execute(Entity entity, ref Habitat habitat)
            {
                habitat.freeSpace = habitat.totalSpace;
                ecb.AddComponent<HasSpace>(entity);
            }
        }
        [WithDisabled(typeof(ActiveTile))]
        private partial struct ClearDeactivatedEmployersJob : IJobEntity
        {
            public EntityCommandBuffer ecb;

            public void Execute(Entity entity, ref Employer employer)
            {
                employer.freeSpace = employer.totalSpace;
                ecb.AddComponent<HasSpace>(entity);
            }
        }
    }
}