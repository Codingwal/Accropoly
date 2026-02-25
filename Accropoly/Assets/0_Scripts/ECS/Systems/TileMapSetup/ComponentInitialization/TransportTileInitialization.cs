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
            foreach (var (tile, entity) in SystemAPI.Query<RefRO<Tile>>().WithEntityAccess().WithAll<NewTile>())
                ecb.AddComponent(entity, new CopyComponent<Tile>(tile.ValueRO));

            // Add ConnectingTile CopyComponent
            foreach (var (connectingTile, entity) in SystemAPI.Query<RefRO<ConnectingTile>>().WithEntityAccess().WithAll<NewTile>())
                ecb.AddComponent(entity, new CopyComponent<ConnectingTile>(connectingTile.ValueRO));

            if (SystemAPI.HasSingleton<LoadGame>())
            {
                // Add Tile CopyComponent
                foreach (var (tile, entity) in SystemAPI.Query<RefRO<Tile>>().WithEntityAccess())
                    ecb.AddComponent(entity, new CopyComponent<Tile>(tile.ValueRO));

                // Add ConnectingTile CopyComponent
                foreach (var (connectingTile, entity) in SystemAPI.Query<RefRO<ConnectingTile>>().WithEntityAccess())
                    ecb.AddComponent(entity, new CopyComponent<ConnectingTile>(connectingTile.ValueRO));
            }
        }
    }
}