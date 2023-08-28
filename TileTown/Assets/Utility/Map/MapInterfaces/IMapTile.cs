using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.Android;

public interface IMapTile
{
    public int X { get; set; }
    public int Y { get; set; }

    public Tile GetTile();
    public void DefaultColor();
    public void PlaceableColor();
    public void NotPlaceableColor();
    public bool CanBePlaced();
}
