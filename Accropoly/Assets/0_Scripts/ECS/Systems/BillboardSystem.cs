using Tags;
using Unity.Entities;
using UnityEngine;
using ConfigComponents;
using Unity.Transforms;
using Components;

public partial class BillboardSystem : SystemBase
{
    EntityQuery billboardQuery;
    protected override void OnCreate()
    {
        billboardQuery = GetEntityQuery(typeof(Billboard));
    }
    protected override void OnUpdate()
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        // Delete all billboards if the game is getting saved
        if (SystemAPI.HasSingleton<SaveGame>())
        {
            ecb.DestroyEntity(billboardQuery, EntityQueryCaptureMode.AtPlayback);
            return;
        }

        // Alternative to RequireForUpdate<RunGame>()
        if (!SystemAPI.HasSingleton<RunGame>()) return;

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
        Entity prefab = SystemAPI.GetSingleton<BillboardPrefab>();
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