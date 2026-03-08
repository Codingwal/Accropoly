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
    private bool firstUpdate = true;

    protected override void OnUpdate()
    {
        // Can't be done in OnCreate because GraphicsSystem needs to finish setup
        if (firstUpdate)
        {
            Appearence data = Authoring.Appearence.CreateAppearenceConfig();
            EntityManager.CreateSingleton(data);
            firstUpdate = false;
        }

        Appearence config = SystemAPI.GetSingleton<Appearence>();

        new UpdateSimpleTilesJob { config = config }
            .Schedule();

        new UpdateConnectingTilesJob
        {
            config = config,
            tileLookup = GetComponentLookup<Tile>(isReadOnly: true),
            tileGrid = TileGridUtility.GetEntityGrid()
        }.Schedule();
    }
    protected override void OnDestroy()
    {
        ECSUtility.GetSingleton<Appearence>().Dispose();
    }
    public void UpdateAppearence(Entity entity, TileType tileType)
    {
        Appearence config = SystemAPI.GetSingleton<Appearence>();

        var info = GetMaterialMeshInfo(tileType, config);
        SystemAPI.SetComponent(entity, info);
    }
    public static MaterialMeshInfo GetMaterialMeshInfo(TileType tileType, Appearence config)
    {
        if (config.simpleTiles.ContainsKey((int)tileType))
            return config.simpleTiles[(int)tileType];
        else if (config.connectingTiles.ContainsKey((int)tileType))
            return config.connectingTiles[(int)tileType].pairs[0];
        else
            throw new();
    }

    [BurstCompile]
    [WithNone(typeof(ConnectingTile))]
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
