using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Tile
{
    public TileType tileType;
    public int direction;

    public Tile(TileType tileType, int direction)
    {
        this.tileType = tileType;
        this.direction = direction;
    }
}
public enum TileType
{
    // Naturally generated
    Plains,
    Forest,

    // Normal street, houses can connect
    Street,
    StreetCorner,

    // // Faster street, houses can't connect
    // Highway,
    // HighwayCorner,
}