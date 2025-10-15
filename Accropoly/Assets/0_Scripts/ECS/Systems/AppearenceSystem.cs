using Unity.Entities;
using Components;
using ConfigComponents;
using Unity.Rendering;
using Tags;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

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

        Entities.WithChangeFilter<Tile>().WithNone<ConnectingTile>().ForEach((ref MaterialMeshInfo data, in Tile tile) =>
        {
            Debug.Assert(config.simpleTiles.ContainsKey((int)tile.tileType), $"{tile.tileType} is not a simple tile.");
            data = config.simpleTiles[(int)tile.tileType];
        }).Schedule();

        Entities.ForEach((ref LocalTransform transform, ref MaterialMeshInfo data, in Tile tile, in ConnectingTile connectingTile) =>
        {
            int index = connectingTile.GetIndex();
            if (index == 5 && tile.tileType == TileType.Lake)
            {
                var tileedges = TileGridUtility.GetSquareEdgeTiles(tile.pos);
                for (int i = 0; i < 4; i++)
                {
                    var edge = tileedges[i];
                    Tile edgetile = SystemAPI.GetComponent<Tile>(edge);
                    if (edgetile.tileType != TileType.Lake)
                    {
                        index = 6;
                        transform.Rotation = quaternion.EulerXYZ(0, ((Direction)i).ToRadians(), 0);
                        break;
                    }
                }
            }
            Debug.Assert(config.connectingTiles.ContainsKey((int)tile.tileType), $"{tile.tileType} is not a connecting tile.");
            data = config.connectingTiles[(int)tile.tileType].pairs[index];
        }).WithoutBurst().Run();
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
}
