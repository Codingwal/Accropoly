using Components;
using Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    /// <summary>
    /// Deletes billboards when
    /// (1) The game is getting saved -> delete all billboards
    /// (2) Tile gets replaced -> delete all billboards of that tile
    /// (3) The problem has been fixed -> remove the corresponding billboard
    /// </summary>
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
                Entities.ForEach((ref BillboardOwner billboardOwner) =>
                {
                    billboardOwner.billboards.Dispose();
                }).Schedule();
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
            Entities.WithChangeFilter<HasElectricity>().WithAll<HasElectricity>().ForEach((Entity entity, ref BillboardOwner billboardOwner, in Tile tile) =>
            {
                RemoveBillboard(entity, tile, ref billboardOwner, BillboardInfo.Problems.NoElectricity, ecb);
            }).Schedule();
            Entities.WithChangeFilter<IsConnected>().WithAll<IsConnected>().ForEach((Entity entity, ref BillboardOwner billboardOwner, in Tile tile) =>
            {
                RemoveBillboard(entity, tile, ref billboardOwner, BillboardInfo.Problems.NotConnected, ecb);
            }).Schedule();
        }
        private static void RemoveBillboard(Entity entity, Tile tile, ref BillboardOwner billboardOwner, BillboardInfo.Problems problem, EntityCommandBuffer ecb)
        {
            // Delete the billboard
            for (int i = 0; i < billboardOwner.billboards.Length; i++)
            {
                var info = billboardOwner.billboards[i];
                if (info.problem == problem)
                {
                    ecb.DestroyEntity(info.entity);
                    billboardOwner.billboards.RemoveAt(i);
                    UpdateBillboards(tile.pos, billboardOwner, ecb);
                }
            }

            // Check if there are no billboards left
            if (billboardOwner.billboards.Length == 0)
            {
                // Dispose the UnsafeList and remove the component
                billboardOwner.billboards.Dispose();
                ecb.RemoveComponent<BillboardOwner>(entity);
            }
        }
        private static void UpdateBillboards(int2 pos, BillboardOwner billboardOwner, EntityCommandBuffer ecb)
        {
            for (int i = 0; i < billboardOwner.billboards.Length; i++)
            {
                Entity billboardEntity = billboardOwner.billboards[i].entity;
                ecb.SetComponent(billboardEntity, LocalTransform.FromPositionRotationScale(new(pos.x * 2, i * 0.7f + 1, pos.y * 2), quaternion.identity, 0.5f));
            }
        }
    }
}