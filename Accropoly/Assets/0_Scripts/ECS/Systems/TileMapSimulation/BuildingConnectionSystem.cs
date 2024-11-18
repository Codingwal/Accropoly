using Unity.Entities;
using Components;
public partial class BuildingConnectionSystem : SystemBase
{
    private int frame;
    protected override void OnCreate()
    {
        RequireForUpdate<RunGameTag>();
        RequireForUpdate<EntityGridHolder>();
    }
    protected override void OnUpdate()
    {
        // Only run this function every 50 frames
        frame++;
        if (frame % 50 != 4) return;

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        var buffer = SystemAPI.GetBuffer<EntityBufferElement>(SystemAPI.GetSingletonEntity<EntityGridHolder>());

        Entities.WithPresent<IsConnectedTag>().ForEach((Entity entity, in Tile mapTileComponent) =>
        {
            bool isConnected = false;
            foreach (Direction direction in Direction.GetDirections())
            {
                if (!TileGridUtility.TryGetTile(mapTileComponent.pos + direction.DirectionVec, buffer, out Entity neighbour)) continue;

                if (SystemAPI.HasComponent<BuildingConnectorTag>(neighbour))
                {
                    isConnected = true;
                    break;
                }
            }
            ecb.SetComponentEnabled<IsConnectedTag>(entity, isConnected);
        }).WithoutBurst().Schedule();

    }
}
