using Tags;
using Unity.Entities;
using UnityEngine;
using ConfigComponents;
using Unity.Transforms;
using Components;

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

        // Delete billboard if tile gets replaced
        Entities.WithAll<Replace>().ForEach((Entity entity, in BillboardOwner billboardOwner) =>
        {
            ecb.DestroyEntity(billboardOwner.billboardEntity);

            ecb.RemoveComponent<BillboardOwner>(entity);
        }).Schedule();

        // Delete billboard if problem is fixed
        Entities.WithAll<HasElectricity>().ForEach((Entity entity, in BillboardOwner billboardOwner) =>
        {
            ecb.DestroyEntity(billboardOwner.billboardEntity);

            ecb.RemoveComponent<BillboardOwner>(entity);
        }).Schedule();

        // Add billboard if there is a problem
        Entities.WithNone<BillboardOwner>().WithDisabled<HasElectricity>().ForEach((Entity tileEntity, in LocalTransform transform) =>
        {
            Entity entity = ecb.Instantiate(prefab);
            LocalTransform transformCopy = transform;
            transformCopy.Position.y = 1;
            ecb.SetComponent(entity, transformCopy);
            ecb.AddComponent<Billboard>(entity);

            ecb.AddComponent(tileEntity, new BillboardOwner { billboardEntity = entity });
        }).Schedule();
    }
}