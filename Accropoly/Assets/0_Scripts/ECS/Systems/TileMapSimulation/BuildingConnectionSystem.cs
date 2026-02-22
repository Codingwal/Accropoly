using Unity.Entities;
using Components;
using Tags;
using Unity.Jobs;
using Unity.Collections;
using System.Linq;

namespace Systems
{
    /// <summary>
    /// Check if buildings which require a connection (to a street for example) are connected
    /// </summary>
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
            if (frame % 50 != 0) return;

            new CheckBuildingConnectionsJob
            {
                ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged),
                buffer = SystemAPI.GetBuffer<EntityBufferElement>(SystemAPI.GetSingletonEntity<EntityGridHolder>()),
                buildingConnectorLookup = GetComponentLookup<BuildingConnector>(true),
            }.Schedule();
        }

        [WithPresent(typeof(IsConnected))]
        private partial struct CheckBuildingConnectionsJob : IJobEntity
        {
            public EntityCommandBuffer ecb;
            public DynamicBuffer<EntityBufferElement> buffer;
            [ReadOnly] public ComponentLookup<BuildingConnector> buildingConnectorLookup;
            public void Execute(Entity entity, in Tile tile)
            {
                bool isConnected = false;
                foreach (Direction direction in Direction.GetDirections())
                {
                    if (!TileGridUtility.TryGetTile(tile.pos + direction.DirectionVec, buffer, out Entity neighbour)) continue;

                    if (buildingConnectorLookup.HasComponent(neighbour))
                    {
                        isConnected = true;
                        break;
                    }
                }
                ecb.SetComponentEnabled<IsConnected>(entity, isConnected);
            }
        }
    }
}