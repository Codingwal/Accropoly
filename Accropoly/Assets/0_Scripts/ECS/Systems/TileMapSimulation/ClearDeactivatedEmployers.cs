using Unity.Entities;
using Components;
using Tags;

namespace Systems
{
    public partial class ClearDeactivatedEmployers : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<RunGame>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            Entities.WithDisabled<ActiveTile>().ForEach((Entity entity, ref Employer employer) =>
            {
                employer.freeSpace = employer.totalSpace;
                ecb.AddComponent<HasSpace>(entity);
            }).Schedule();
        }
    }
}
