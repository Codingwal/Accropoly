using Components;
using Tags;
using Unity.Entities;
using UnityEngine;

namespace Systems
{
    public partial class LinkBillboards : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<RunGame>();
        }
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            Entities.ForEach((Entity entity, ref BillboardOwner billboardOwner, in BillboardInfo billboardInfo) =>
            {
                billboardOwner.billboards.Add(billboardInfo);
                ecb.RemoveComponent<BillboardInfo>(entity);
            }).Schedule();
        }
    }
}
