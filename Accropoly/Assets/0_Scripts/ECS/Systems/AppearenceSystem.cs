using Unity.Entities;
using Components;
using ConfigComponents;
using Unity.Rendering;
using Tags;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial class AppearenceSystem : SystemBase
{
    private Appearence configCopy; // Needed so that the unmanaged data structures can be disposed
    private bool firstUpdate = true;
    protected override void OnUpdate()
    {
        if (firstUpdate)
        {
            Appearence data = Authoring.Appearence.CreateAppearenceConfig();
            EntityManager.CreateSingleton(data);
            configCopy = data;
            firstUpdate = false;
        }

        if (!(SystemAPI.HasSingleton<RunGame>() || SystemAPI.HasSingleton<LoadGame>()))
            return;

        Appearence config = SystemAPI.GetSingleton<Appearence>();

        new UpdateSimpleTilesJob { config = config }
            .Schedule(SystemAPI.QueryBuilder().WithAll<Tile, MaterialMeshInfo>().WithNone<ConnectingTile>().Build());

        new UpdateConnectingTilesJob
        {
            config = config,
            tileLookup = GetComponentLookup<Tile>(isReadOnly: true),
            tileGrid = TileGridUtility.GetEntityGrid()
        }.Schedule(SystemAPI.QueryBuilder().WithAll<Tile, MaterialMeshInfo, ConnectingTile, LocalTransform>().Build());
    }
    protected override void OnDestroy()
    {
        // Can't use the config singleton as it already has been destroyed

        configCopy.simpleTiles.Dispose();
        foreach (var pair in configCopy.connectingTiles)
        {
            pair.Value.pairs.Dispose();
        }
        configCopy.connectingTiles.Dispose();
    }
    public void UpdateAppearence(Entity entity, TileType tileType)
    {
        Appearence config = SystemAPI.GetSingleton<Appearence>();

        if (config.simpleTiles.TryGetValue((int)tileType, out MaterialMeshInfo newData)) // Is a simple tile
            SystemAPI.SetComponent(entity, newData);
        else // Is a connecting tile
            SystemAPI.SetComponent(entity, config.connectingTiles[(int)tileType].pairs[0]);
    }

    [BurstCompile]
    private partial struct UpdateSimpleTilesJob : IJobEntity
    {
        public Appearence config;
        public void Execute(ref MaterialMeshInfo data, in Tile tile)
        {
            Debug.Assert(config.simpleTiles.ContainsKey((int)tile.tileType), $"{tile.tileType} is not a simple tile.");
            data = config.simpleTiles[(int)tile.tileType];
        }
    }

    [BurstCompile]
    private partial struct UpdateConnectingTilesJob : IJobEntity
    {
        public Appearence config;
        [ReadOnly] public ComponentLookup<Tile> tileLookup;
        public DynamicBuffer<EntityBufferElement> tileGrid;
        public void Execute(ref LocalTransform transform, ref MaterialMeshInfo data, in Tile tile, in ConnectingTile connectingTile)
        {
            int index = connectingTile.GetIndex();
            if (index == 5 && tile.tileType == TileType.Lake)
            {
                Direction dir = Directions.North;
                foreach (var edge in TileGridUtility.GetSquareEdgeTiles(tile.pos, tileGrid))
                {
                    Tile edgeTile = tileLookup[edge];
                    if (edgeTile.tileType != TileType.Lake)
                    {
                        index = 6;
                        transform.Rotation = quaternion.EulerXYZ(0, dir.ToRadians(), 0);
                        break;
                    }
                    dir.Rotate(1);
                }
            }
            Debug.Assert(config.connectingTiles.ContainsKey((int)tile.tileType), $"{tile.tileType} is not a connecting tile.");
            data = config.connectingTiles[(int)tile.tileType].pairs[index];
        }
    }
}
