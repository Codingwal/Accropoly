using Unity.Entities;
using Components;
using ConfigComponents;
using Unity.Rendering;
using Tags;
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

        if (!SystemAPI.HasSingleton<RunGame>())
            return;

        Appearence config = SystemAPI.GetSingleton<Appearence>();

        Entities.WithChangeFilter<Tile>().WithNone<ConnectingTile>().ForEach((ref MaterialMeshInfo data, in Tile tile) =>
        {
            data = config.simpleTiles[(int)tile.tileType];
        }).Schedule();

        Entities.WithChangeFilter<Tile, ConnectingTile>().ForEach((ref MaterialMeshInfo data, in Tile tile, in ConnectingTile connectingTile) =>
        {
            int index = connectingTile.GetIndex();
            data = config.connectingTiles[(int)tile.tileType].pairs[index];
        }).Schedule();
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
