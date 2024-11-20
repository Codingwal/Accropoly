using Tags;
using Unity.Entities;
using UnityEngine;
using ConfigComponents;
using Unity.Transforms;
using Components;
using Unity.Collections;

using BillboardProblems = Components.BillboardOwner.BillboardInfo.Problems;
using System.Linq;

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

        // Delete billboards if tile gets replaced
        Entities.WithAll<Replace>().ForEach((Entity entity, in BillboardOwner billboardOwner) =>
        {
            // Destroy all billboards
            foreach (var info in billboardOwner.billboards)
                ecb.DestroyEntity(info.entity);

            // Dispose the NativeList
            billboardOwner.billboards.Dispose();
        }).Schedule();

        // Delete billboard if problem is fixed
        Entities.WithAll<HasElectricity>().ForEach((Entity entity, ref BillboardOwner billboardOwner) =>
        {
            RemoveBillboard(ref billboardOwner.billboards, BillboardProblems.NoElectricity, ecb);

            // Check if there are no billboards left
            if (billboardOwner.billboards.Length == 0)
                ecb.RemoveComponent<BillboardOwner>(entity);
        }).Schedule();

        // Add billboard if there is a problem
        Entity prefab = SystemAPI.GetSingleton<BillboardPrefab>();
        Entities.WithNone<BillboardOwner>().WithDisabled<HasElectricity>().ForEach((Entity tileEntity, in LocalTransform transform) =>
        {
            Entity newBillboard = AddBillboard(transform, prefab, ecb);
            ecb.AddComponent(tileEntity, new BillboardOwner(new(newBillboard, BillboardProblems.NoElectricity)));
        }).Schedule();
        Entities.WithDisabled<HasElectricity>().ForEach((Entity tileEntity, ref BillboardOwner billboardOwner, in LocalTransform transform) =>
        {
            if (ContainsProblem(billboardOwner.billboards, BillboardProblems.NoElectricity)) return; // Return if this problem has already been added

            // Create billboard and add it to the billboardOwner list
            Entity newBillboard = AddBillboard(transform, prefab, ecb);
            billboardOwner.billboards.Add(new(newBillboard, BillboardProblems.NoElectricity));
        }).Schedule();
    }
    private static Entity AddBillboard(LocalTransform ownerTransform, Entity billboardPrefab, EntityCommandBuffer ecb)
    {
        Entity entity = ecb.Instantiate(billboardPrefab); // Create the billboard
        ownerTransform.Position.y = 1; // Use owner transform but position in the air over it
        ecb.SetComponent(entity, ownerTransform);
        ecb.AddComponent<Billboard>(entity); // Tag component used by queries
        return entity;
    }
    private static bool ContainsProblem(in NativeList<BillboardOwner.BillboardInfo> billboards, BillboardProblems problem)
    {
        for (int i = 0; i < billboards.Length; i++)
            if (billboards[i].problem == problem)
                return true;
        return false;
    }
    private static void RemoveBillboard(ref NativeList<BillboardOwner.BillboardInfo> billboards, BillboardProblems problem, EntityCommandBuffer ecb)
    {
        for (int i = 0; i < billboards.Length; i++)
        {
            var info = billboards[i];
            if (info.problem == problem)
            {
                ecb.DestroyEntity(info.entity);
                billboards.RemoveAt(i);
            }
        }
    }
}