using Unity.Entities;
using Components;
using Unity.Burst;

namespace Systems
{
    /// <summary>
    /// Initialize transport tiles (add components containing old data, used to check if the waypoints need to be updated)
    /// </summary>
    [UpdateInGroup(typeof(ComponentInitializationSystemGroup))]
    public partial struct TransportTileInitialization : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndComponentInitializationECBSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (_, entity) in SystemAPI.Query<RefRO<TransportTile>>().WithEntityAccess()
                .WithNone<CopyComponent<Tile>, CopyComponent<ConnectingTile>>())
            {
                ecb.AddComponent<CopyComponent<Tile>>(entity);
                ecb.AddComponent<CopyComponent<ConnectingTile>>(entity);
            }
        }
    }
}