using Components;
using Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    [UpdateBefore(typeof(CreateBillboards))]
    public partial class LinkBillboards : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<RunGame>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            Entities.ForEach((Entity entity, ref BillboardOwner billboardOwner, in BillboardInfo billboardInfo, in Tile tile) =>
            {
                billboardOwner.billboards.Add(billboardInfo);
                UpdateBillboards(tile.pos, billboardOwner, ecb);
                ecb.RemoveComponent<BillboardInfo>(entity);
            }).Schedule();
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
