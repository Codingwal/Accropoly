using Tags;
using Unity.Entities;
using ConfigComponents;
using Unity.Transforms;
using Components;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;


namespace Systems
{
    public partial class CreateBillboards : SystemBase
    {
        const BillboardInfo.Problems NoElectricityProblem = BillboardInfo.Problems.NoElectricity;
        protected override void OnCreate()
        {
            RequireForUpdate<RunGame>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            // Add billboard if there is a problem and the tile is not a billboard owner
            Entity prefab = SystemAPI.GetSingleton<BillboardPrefab>();
            Entities.WithNone<BillboardOwner>().WithDisabled<HasElectricity>().ForEach((Entity tileEntity, in LocalTransform transform) =>
            {
                Entity newBillboard = AddBillboard(transform, prefab, ecb);
                ecb.AddComponent(tileEntity, new BillboardInfo(newBillboard, NoElectricityProblem)); // This will get added to the billboards list
                ecb.AddComponent(tileEntity, BillboardOwner.CreateInstance());
            }).Schedule();

            // Add billboard if there is a problem and the tile is a billboard owner
            Entities.WithChangeFilter<HasElectricity>().WithDisabled<HasElectricity>().ForEach((Entity tileEntity, ref BillboardOwner billboardOwner, in LocalTransform transform) =>
            {
                if (ContainsProblem(billboardOwner.billboards, NoElectricityProblem)) return; // Return if this problem has already been added

                // Create billboard and add it to the billboardOwner list
                Entity newBillboard = AddBillboard(transform, prefab, ecb);
                billboardOwner.billboards.Add(new(newBillboard, NoElectricityProblem));
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
        private static bool ContainsProblem(in UnsafeList<BillboardInfo> billboards, BillboardInfo.Problems problem)
        {
            for (int i = 0; i < billboards.Length; i++)
                if (billboards[i].problem == problem)
                    return true;
            return false;
        }
    }
}