using Unity.Entities;
using Components;
using Tags;

namespace Systems
{
    /// <summary>
    /// Initialize transport tiles (add components containing old data, used to check if the waypoints need to be updated)
    /// </summary>
    [UpdateInGroup(typeof(ComponentInitializationSystemGroup))]
    public partial class TransportTileInitialization : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndComponentInitializationECBSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            // Add Tile CopyComponent
            foreach (var (_, entity) in SystemAPI.Query<RefRO<Tile>>().WithEntityAccess().WithAll<NewTile, TransportTile>())
                ecb.AddComponent<CopyComponent<Tile>>(entity);

            // Add ConnectingTile CopyComponent
            foreach (var (_, entity) in SystemAPI.Query<RefRO<ConnectingTile>>().WithEntityAccess().WithAll<NewTile, TransportTile>())
                ecb.AddComponent<CopyComponent<ConnectingTile>>(entity);

            if (SystemAPI.HasSingleton<LoadGame>())
            {
                // Add Tile CopyComponent
                foreach (var (_, entity) in SystemAPI.Query<RefRO<Tile>>().WithEntityAccess().WithAll<TransportTile>())
                    ecb.AddComponent<CopyComponent<Tile>>(entity);

                // Add ConnectingTile CopyComponent
                foreach (var (_, entity) in SystemAPI.Query<RefRO<ConnectingTile>>().WithEntityAccess().WithAll<TransportTile>())
                    ecb.AddComponent<CopyComponent<ConnectingTile>>(entity);
            }
        }
    }
}