using Unity.Entities;
using Components;
using Tags;

public partial class BuildingConnectionSystem : SystemBase
{
    private int frame;
    protected override void OnCreate()
    {
        RequireForUpdate<RunGame>();
        RequireForUpdate<EntityGridHolder>();
    }
    protected override void OnUpdate()
    {
        // Only run this function every 50 frames
        frame++;
        if (frame % 50 != 4) return;

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        var buffer = SystemAPI.GetBuffer<EntityBufferElement>(SystemAPI.GetSingletonEntity<EntityGridHolder>());

        Entities.WithPresent<IsConnected>().ForEach((Entity entity, in Tile mapTileComponent) =>
        {
            bool isConnected = false;
            foreach (Direction direction in Direction.GetDirections())
            {
                if (!TileGridUtility.TryGetTile(mapTileComponent.pos + direction.DirectionVec, buffer, out Entity neighbour)) continue;

                if (SystemAPI.HasComponent<BuildingConnector>(neighbour))
                {
                    isConnected = true;
                    break;
                }
            }
            ecb.SetComponentEnabled<IsConnected>(entity, isConnected);
        }).WithoutBurst().Schedule();

    }
}
