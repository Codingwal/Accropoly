using Unity.Entities;

public partial class BuildingConnectionSystem : SystemBase
{
    private uint frame;
    protected override void OnUpdate()
    {
        // Only run this function every 50 frames
        frame++;
        if (frame % 50 != 1) return;

        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        Entities.WithPresent<IsConnectedTag>().ForEach((Entity entity, in MapTileComponent mapTileComponent) =>
        {
            bool isConnected = false;
            foreach (Direction direction in Direction.GetDirections())
            {
                Entity neighbour = TileGridUtility.TryGetTile(mapTileComponent.pos + direction.DirectionVec, out bool neighbourExists);

                if (!neighbourExists) continue;
                if (!SystemAPI.HasComponent<BuildingConnector>(neighbour)) continue;

                var buildingConnector = SystemAPI.GetComponent<BuildingConnector>(neighbour);
                Direction neighbourRotation = SystemAPI.GetComponent<MapTileComponent>(neighbour).rotation;

                if (buildingConnector.CanConnect(direction.Flip(), neighbourRotation))
                {
                    isConnected = true;
                    break;
                }
            }
            ecb.SetComponentEnabled<IsConnectedTag>(entity, isConnected);
        }).WithoutBurst().Schedule();

    }
}
