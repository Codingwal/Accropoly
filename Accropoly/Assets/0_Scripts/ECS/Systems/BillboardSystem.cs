using Tags;
using Unity.Entities;
using UnityEngine;
using Components;
using ConfigComponents;
using Unity.Transforms;

public partial class BillboardSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<RunGame>();
    }
    protected override void OnUpdate()
    {
        Entity prefab = SystemAPI.GetSingleton<BillboardPrefab>();
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        Entities.WithNone<HasBillboard>().WithDisabled<HasElectricity>().ForEach((Entity tileEntity, in LocalTransform transform) =>
        {
            ecb.AddComponent<HasBillboard>(tileEntity);

            Entity entity = ecb.Instantiate(prefab);
            LocalTransform transformCopy = transform;
            transformCopy.Position.y = 1;
            ecb.SetComponent(entity, transformCopy);
            ecb.AddComponent<Billboard>(entity);
        }).Schedule();
    }
}
namespace Tags
{
    public struct Billboard : IComponentData { }
    public struct HasBillboard : IComponentData { }
}