using Tags;
using Unity.Entities;
using ConfigComponents;
using Unity.Transforms;
using Components;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Unity.Collections;


namespace Systems
{
    public partial class CreateBillboards : SystemBase
    {
        const BillboardInfo.Problems NoElectricityProblem = BillboardInfo.Problems.NoElectricity;
        const BillboardInfo.Problems NotConnectedProblem = BillboardInfo.Problems.NotConnected;

        protected override void OnCreate()
        {
            RequireForUpdate<RunGame>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            Entity prefab = SystemAPI.GetSingleton<BillboardPrefab>();
            NativeList<Entity> alreadyVisitedEntities = new(Allocator.TempJob);

            // Add billboard if there is a problem and the tile is not a billboard owner
            Entities.WithNone<BillboardOwner>().WithDisabled<HasElectricity>().ForEach((Entity tileEntity, in LocalTransform transform) =>
            {
                AddBillboardNoBillboardOwner(tileEntity, transform, NoElectricityProblem, prefab, ecb, alreadyVisitedEntities);
            }).Schedule();
            Entities.WithNone<BillboardOwner>().WithDisabled<IsConnected>().ForEach((Entity tileEntity, in LocalTransform transform) =>
            {
                AddBillboardNoBillboardOwner(tileEntity, transform, NotConnectedProblem, prefab, ecb, alreadyVisitedEntities);
            }).Schedule();

            // Add billboard if there is a problem and the tile is a billboard owner
            Entities.WithDisabled<HasElectricity>().ForEach((Entity tileEntity, ref BillboardOwner billboardOwner, in LocalTransform transform) =>
            {
                AddBillboardToBillboardOwner(tileEntity, ref billboardOwner, transform, NoElectricityProblem, prefab, ecb, alreadyVisitedEntities);
            }).Schedule();
            Entities.WithDisabled<IsConnected>().ForEach((Entity tileEntity, ref BillboardOwner billboardOwner, in LocalTransform transform) =>
            {
                AddBillboardToBillboardOwner(tileEntity, ref billboardOwner, transform, NotConnectedProblem, prefab, ecb, alreadyVisitedEntities);
            }).Schedule();

            alreadyVisitedEntities.Dispose(Dependency);
        }
        private static void AddBillboardNoBillboardOwner(Entity tileEntity, in LocalTransform transform, BillboardInfo.Problems problem, Entity prefab, EntityCommandBuffer ecb, NativeList<Entity> alreadyVisitedEntities)
        {
            if (alreadyVisitedEntities.Contains(tileEntity)) return;

            Entity newBillboard = AddBillboard(transform, (int)problem + 1, prefab, ecb);
            ecb.AddComponent(tileEntity, new BillboardInfo(newBillboard, problem)); // This will get added to the billboards list
            ecb.AddComponent(tileEntity, BillboardOwner.CreateInstance());

            alreadyVisitedEntities.Add(tileEntity);
        }
        private static void AddBillboardToBillboardOwner(Entity tileEntity, ref BillboardOwner billboardOwner, in LocalTransform transform, BillboardInfo.Problems problem, Entity prefab, EntityCommandBuffer ecb, NativeList<Entity> alreadyVisitedEntities)
        {
            if (alreadyVisitedEntities.Contains(tileEntity)) return;
            if (ContainsProblem(billboardOwner.billboards, problem)) return; // Return if this problem has already been added

            Entity newBillboard = AddBillboard(transform, (int)problem + 1, prefab, ecb);
            ecb.AddComponent(tileEntity, new BillboardInfo(newBillboard, problem)); // This will get added to the billboards list

            alreadyVisitedEntities.Add(tileEntity);
        }
        private static Entity AddBillboard(LocalTransform ownerTransform, float height, Entity billboardPrefab, EntityCommandBuffer ecb)
        {
            Entity entity = ecb.Instantiate(billboardPrefab); // Create the billboard
            ownerTransform.Position.y = height; // Use owner transform but position in the air over it
            ecb.SetComponent(entity, ownerTransform);
            ecb.AddComponent<Billboard>(entity); // Tag component used by queries
            return entity;
        }
        private static bool ContainsProblem(in UnsafeList<BillboardInfo> billboards, BillboardInfo.Problems problem)
        {
            for (int i = 0; i < billboards.Length; i++)
                if (billboards[i].problem == problem)
                    return true;
            return false;
        }
    }
}