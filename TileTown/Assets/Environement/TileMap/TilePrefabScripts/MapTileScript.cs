using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTileScript : MonoBehaviour, IMapTile
{
    public TileType tile;

    public TileType GetTileType()
    {
        return tile;
    }
}
