using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMapTile
{
    public Tile GetTile();
    public void DefaultColor();
    public void PlaceableColor();
    public void NotPlaceableColor();
}
