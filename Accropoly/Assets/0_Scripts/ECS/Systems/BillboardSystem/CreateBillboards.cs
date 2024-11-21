using Tags;
using Unity.Entities;
using ConfigComponents;
using Unity.Transforms;
using Components;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Unity.Collections;
using Unity.Rendering;


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
            var config = SystemAPI.GetSingleton<Billboarding>();
            NativeList<Entity> alreadyVisitedEntities = new(Allocator.TempJob);

            // Add billboard if there is a problem and the tile is not a billboard owner
            Entities.WithNone<BillboardOwner>().WithDisabled<HasElectricity>().ForEach((Entity tileEntity, in LocalTransform transform) =>
            {
                AddBillboardNoBillboardOwner(tileEntity, transform, NoElectricityProblem, config, ecb, alreadyVisitedEntities);
            }).Schedule();
            Entities.WithNone<BillboardOwner>().WithDisabled<IsConnected>().ForEach((Entity tileEntity, in LocalTransform transform) =>
            {
                AddBillboardNoBillboardOwner(tileEntity, transform, NotConnectedProblem, config, ecb, alreadyVisitedEntities);
            }).Schedule();

            // Add billboard if there is a problem and the tile is a billboard owner
            Entities.WithDisabled<HasElectricity>().ForEach((Entity tileEntity, ref BillboardOwner billboardOwner, in LocalTransform transform) =>
            {
                AddBillboardToBillboardOwner(tileEntity, ref billboardOwner, transform, NoElectricityProblem, config, ecb, alreadyVisitedEntities);
            }).Schedule();
            Entities.WithDisabled<IsConnected>().ForEach((Entity tileEntity, ref BillboardOwner billboardOwner, in LocalTransform transform) =>
            {
                AddBillboardToBillboardOwner(tileEntity, ref billboardOwner, transform, NotConnectedProblem, config, ecb, alreadyVisitedEntities);
            }).Schedule();

            alreadyVisitedEntities.Dispose(Dependency);
        }
        private static void AddBillboardNoBillboardOwner(Entity tileEntity, in LocalTransform transform, BillboardInfo.Problems problem, Billboarding config, EntityCommandBuffer ecb, NativeList<Entity> alreadyVisitedEntities)
        {
            if (alreadyVisitedEntities.Contains(tileEntity)) return;

            Entity newBillboard = AddBillboard(transform, problem, config, ecb);
            ecb.AddComponent(tileEntity, new BillboardInfo(newBillboard, problem)); // This will get added to the billboards list
            ecb.AddComponent(tileEntity, BillboardOwner.CreateInstance());

            alreadyVisitedEntities.Add(tileEntity);
        }
        private static void AddBillboardToBillboardOwner(Entity tileEntity, ref BillboardOwner billboardOwner, in LocalTransform transform, BillboardInfo.Problems problem, Billboarding config, EntityCommandBuffer ecb, NativeList<Entity> alreadyVisitedEntities)
        {
            if (alreadyVisitedEntities.Contains(tileEntity)) return;
            if (ContainsProblem(billboardOwner.billboards, problem)) return; // Return if this problem has already been added

            Entity newBillboard = AddBillboard(transform, problem, config, ecb);
            ecb.AddComponent(tileEntity, new BillboardInfo(newBillboard, problem)); // This will get added to the billboards list

            alreadyVisitedEntities.Add(tileEntity);
        }
        private static Entity AddBillboard(LocalTransform ownerTransform, BillboardInfo.Problems problem, Billboarding config, EntityCommandBuffer ecb)
        {
            // Create the billboard using the prefab
            Entity entity = ecb.Instantiate(config.prefab);

            // Use owner transform but position in the air over it
            ownerTransform.Position.y = (int)problem + 1;
            ownerTransform.Scale = 0.5f;
            ecb.SetComponent(entity, ownerTransform);
            ecb.AddComponent<Billboard>(entity); // Tag component used by queries

            // Set material and mesh according to problem
            ecb.SetComponent(entity, new MaterialMeshInfo(config.materialIDs[(int)problem], config.meshID));

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