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
    public static bool operator ==(Tile left, Tile right)
    {
        return left.tileType == right.tileType && left.direction == right.direction;
    }
    public static bool operator !=(Tile left, Tile right)
    {
        return !(left.tileType == right.tileType && left.direction == right.direction);
    }
    public override readonly bool Equals(object obj)
    {
        return base.Equals(obj);
    }
    public override readonly int GetHashCode()
    {
        return base.GetHashCode();
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
    StreetTJunction,
    StreetJunction,

    House

    // // Faster street, houses can't connect
    // Highway,
    // HighwayCorner,
}