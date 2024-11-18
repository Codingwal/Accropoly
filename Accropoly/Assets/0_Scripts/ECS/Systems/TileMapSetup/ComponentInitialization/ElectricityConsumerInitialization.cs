using Unity.Entities;
using Components;
using Tags;

namespace Systems
{
    [UpdateInGroup(typeof(ComponentInitializationSystemGroup))]
    public partial class ElectricityConsumerInitialization : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndComponentInitializationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            Entities.WithAll<NewTile, ElectricityConsumer>().ForEach((Entity entity) =>
            {
                ecb.AddComponent<HasElectricity>(entity);
                ecb.SetComponentEnabled<HasElectricity>(entity, false);
            }).Schedule();

            if (SystemAPI.HasSingleton<LoadGame>())
            {
                Entities.WithAll<ElectricityConsumer>().ForEach((Entity entity) =>
                {
                    ecb.AddComponent<HasElectricity>(entity);
                    ecb.SetComponentEnabled<HasElectricity>(entity, false);
                }).Schedule();
            }
        }
    }
}