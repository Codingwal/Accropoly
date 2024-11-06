using Unity.Entities;
using UnityEngine;

public partial class BuildingConnectionSystem : SystemBase
{
    private uint frame;
    protected override void OnUpdate()
    {
        // Only run this function every 50 frames
        frame++;
        if (frame % 50 != 1) return;

        Entities.WithPresent<IsConnectedTag>().ForEach((Entity entity, in MapTileComponent mapTileComponent) =>
        {
            bool isConnected = false;
            foreach (Direction direction in Direction.GetDirections())
            {
                Entity neighbour = TileGridUtility.TryGetTile(mapTileComponent.pos + direction.DirectionVec, out bool neighbourExists);

                if (!neighbourExists) continue;
                if (!EntityManager.HasComponent<BuildingConnector>(neighbour)) continue;

                var buildingConnector = EntityManager.GetComponentData<BuildingConnector>(neighbour);
                int neighbourRotation = EntityManager.GetComponentData<MapTileComponent>(neighbour).rotation / 90 % 4;
                Debug.Log(neighbourRotation);

                if (buildingConnector.CanConnect(direction.Flip(), neighbourRotation))
                {
                    isConnected = true;
                    break;
                }
            }
            EntityManager.SetComponentEnabled<IsConnectedTag>(entity, isConnected);
        }).WithoutBurst().Run();

    }
}
