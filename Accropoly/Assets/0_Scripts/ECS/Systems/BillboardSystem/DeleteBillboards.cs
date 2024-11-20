using Components;
using Tags;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace Systems
{
    public partial class DeleteBillboards : SystemBase
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

                // Dispose the UnsafeList
                billboardOwner.billboards.Dispose();
            }).Schedule();

            // Delete billboard if problem is fixed
            Entities.WithAll<HasElectricity>().ForEach((Entity entity, ref BillboardOwner billboardOwner) =>
            {
                RemoveBillboard(ref billboardOwner.billboards, BillboardInfo.Problems.NoElectricity, ecb);

                // Check if there are no billboards left
                if (billboardOwner.billboards.Length == 0)
                {
                    billboardOwner.billboards.Dispose();
                    ecb.RemoveComponent<BillboardOwner>(entity);
                }
            }).Schedule();
        }
        private static void RemoveBillboard(ref UnsafeList<BillboardInfo> billboards, BillboardInfo.Problems problem, EntityCommandBuffer ecb)
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
}