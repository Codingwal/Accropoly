using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetTile : MapTileScript, IHouseConnectable
{
    private List<Vector2> _arableTiles;
    public List<Vector2> ArableTiles
    {
        get
        {
            _arableTiles = new();
            switch (tileType)
            {
                case TileType.Street:
                    _arableTiles.Add(MapHandler.GetTilePosFromNeighbour(TilePos, transform.eulerAngles.y + 90));
                    _arableTiles.Add(MapHandler.GetTilePosFromNeighbour(TilePos, transform.eulerAngles.y + 270));
                    break;
                case TileType.StreetCorner:
                    _arableTiles.Add(MapHandler.GetTilePosFromNeighbour(TilePos, transform.eulerAngles.y + 270));
                    _arableTiles.Add(MapHandler.GetTilePosFromNeighbour(TilePos, transform.eulerAngles.y + 180));
                    break;

                case TileType.StreetTJunction:
                    _arableTiles.Add(MapHandler.GetTilePosFromNeighbour(TilePos, transform.eulerAngles.y + 270));
                    break;
            }
            return _arableTiles;
        }

    }
}
