using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMapTile
{
    public Tile GetTile();
    public void PlaceableColor();
    public void NotPlaceableColor();
}
