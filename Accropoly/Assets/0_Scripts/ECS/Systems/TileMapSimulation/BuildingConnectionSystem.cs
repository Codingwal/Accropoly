using Unity.Entities;
using Components;
using Tags;
using Unity.Jobs;
using Unity.Collections;
using System.Linq;
using NUnit.Framework;
using Unity.Burst;

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

        [BurstCompile]
        [WithPresent(typeof(IsConnected))]
        private partial struct CheckBuildingConnectionsJob : IJobEntity
        {
            public EntityCommandBuffer ecb;
            public DynamicBuffer<EntityBufferElement> buffer;
            [ReadOnly] public ComponentLookup<BuildingConnector> buildingConnectorLookup;
            public void Execute(in Tile tile, EnabledRefRW<IsConnected> isConnected)
            {
                foreach (Direction direction in Direction.GetDirections())
                {
                    if (!TileGridUtility.TryGetTile(tile.pos + direction.DirectionVec, buffer, out Entity neighbour)) continue;

                    if (buildingConnectorLookup.HasComponent(neighbour))
                    {
                        isConnected.ValueRW = true;
                        return;
                    }
                }
                isConnected.ValueRW = false;
            }
        }
    }
}