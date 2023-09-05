using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.Android;

public interface IMapTile
{
    public int X { get; set; }
    public int Y { get; set; }
    public Vector2 TilePos { get; }

    public void Init();
    public void Load();
    public Tile GetTile();

    public void DefaultColor();
    public void PlaceableColor();
    public void NotPlaceableColor();
    public event Action ChildsDefaultColor;
    public event Action ChildsPlaceableColor;
    public event Action ChildsNotPlaceableColor;

    public bool CanBePlaced();
    public bool CanPersist();
    public void OnRemove();

    public void OnMouseEnterChild();
    public void OnMouseExitChild();
}
