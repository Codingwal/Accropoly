using Unity.Entities;

public partial class BuildingConnectionSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<RunGameTag>();
        RequireForUpdate<EntityGridHolder>();
    }
    protected override void OnUpdate()
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        var buffer = SystemAPI.GetBuffer<EntityBufferElement>(SystemAPI.GetSingletonEntity<EntityGridHolder>());

        Entities.WithPresent<IsConnectedTag>().ForEach((Entity entity, in MapTileComponent mapTileComponent) =>
        {
            bool isConnected = false;
            foreach (Direction direction in Direction.GetDirections())
            {
                Entity neighbour = TileGridUtility.TryGetTile(mapTileComponent.pos + direction.DirectionVec, buffer, out bool neighbourExists);

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
