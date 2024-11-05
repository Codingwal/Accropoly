using Unity.Entities;

public partial class BuildingConnectionSystem : SystemBase
{
    private uint frame;
    protected override void OnUpdate()
    {
        // Only run this function every 50 frames
        frame++;
        if (frame % 500 != 0) return;

        Entities.WithPresent<IsConnectedTag>().ForEach((Entity entity, in MapTileComponent mapTileComponent) =>
        {
            bool isConnected = false;
            foreach (Direction direction in Direction.GetDirections())
            {
                Entity neighbour = TileGridUtility.TryGetTile(mapTileComponent.pos + direction.DirectionVec, out bool neighbourExists);

                if (!neighbourExists) continue;
                if (!EntityManager.HasComponent<BuildingConnector>(neighbour)) continue;

                var buildingConnector = EntityManager.GetComponentData<BuildingConnector>(neighbour);

                if (buildingConnector.CanConnect(direction.Flip()))
                {
                    isConnected = true;
                    break;
                }
            }
            EntityManager.SetComponentEnabled<IsConnectedTag>(entity, isConnected);
        }).WithoutBurst().Run();

    }
}
