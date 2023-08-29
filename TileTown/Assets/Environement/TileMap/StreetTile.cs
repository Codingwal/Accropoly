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
                    switch (transform.eulerAngles.y)
                    {
                        case 0:
                        case 180:
                            _arableTiles.Add(new(X + 1, Y));
                            _arableTiles.Add(new(X - 1, Y));
                            break;
                        case 90:
                        case 270:
                            _arableTiles.Add(new(X, Y + 1));
                            _arableTiles.Add(new(X, Y - 1));
                            break;
                    }
                    break;
                case TileType.StreetCorner:
                    switch (transform.eulerAngles.y)
                    {
                        case 0:
                            _arableTiles.Add(new(X - 1, Y));
                            _arableTiles.Add(new(X, Y - 1));
                            break;
                        case 90:
                            _arableTiles.Add(new(X - 1, Y));
                            _arableTiles.Add(new(X, Y + 1));
                            break;
                        case 180:
                            _arableTiles.Add(new(X + 1, Y));
                            _arableTiles.Add(new(X, Y + 1));
                            break;
                        case 270:
                            _arableTiles.Add(new(X + 1, Y));
                            _arableTiles.Add(new(X, Y - 1));
                            break;
                    }
                    break;
                case TileType.StreetTJunction:
                    switch (transform.eulerAngles.y)
                    {
                        case 0:
                            _arableTiles.Add(new(X - 1, Y));
                            break;
                        case 90:
                            _arableTiles.Add(new(X, Y + 1));
                            break;
                        case 180:
                            _arableTiles.Add(new(X + 1, Y));
                            break;
                        case 270:
                            _arableTiles.Add(new(X, Y - 1));
                            break;
                    }
                    break;
            }
            return _arableTiles;
        }

    }
}
