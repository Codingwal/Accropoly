using Unity.Entities;
using Components;
using Tags;
using Unity.Collections;

namespace Systems
{
    /// <summary>
    /// Initialize electricty consumers (Add but disable HasElectricity tag)
    /// </summary>
    [UpdateInGroup(typeof(ComponentInitializationSystemGroup))]
    public partial class ElectricityConsumerInitialization : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndComponentInitializationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            EntityQuery newConsumers = SystemAPI.QueryBuilder().WithAll<NewTile, ElectricityConsumer>().Build();
            AddDisabledHasElectricityComponent(newConsumers);

            void AddDisabledHasElectricityComponent(EntityQuery query)
            {
                // Add tag component
                ecb.AddComponent<HasElectricity>(query, EntityQueryCaptureMode.AtPlayback);

                // Disable tag component
                var array = query.ToEntityArray(Allocator.Temp);
                foreach (var entity in array)
                    ecb.SetComponentEnabled<HasElectricity>(entity, false);
                array.Dispose();
            }
        }
    }
}