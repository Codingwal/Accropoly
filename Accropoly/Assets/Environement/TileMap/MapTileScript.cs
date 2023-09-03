using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class MapTileScript : MonoBehaviour, IMapTile
{
    [SerializeField] protected TileType tileType;

    // Position / Index
    public int X { get; set; }
    public int Y { get; set; }
    public Vector2 TilePos
    {
        get { return new(X, Y); }
    }


    public event Action ChildsDefaultColor;
    public event Action ChildsPlaceableColor;
    public event Action ChildsNotPlaceableColor;

    // References
    private BuildingSystemHandler buildingSystemHandler;

    private void Awake()
    {
        buildingSystemHandler = BuildingSystemHandler.Instance;
    }
    private void OnEnable()
    {
        TownManager.Instance.CalculateExpenditure += CalculateExpenditure;
    }
    private void OnDisable()
    {
        TownManager.Instance.CalculateExpenditure -= CalculateExpenditure;
    }
    public void OnMouseEnterChild()
    {
        if (this == null)
        {
            return;
        }
        buildingSystemHandler.selectedTile = transform;
    }
    public void OnMouseExitChild()
    {
        DefaultColor();

        if (buildingSystemHandler.selectedTile == transform)
        {
            buildingSystemHandler.selectedTile = null;
        }
    }

    private void CalculateExpenditure(ref float expenditure)
    {
        if (!TownManager.Instance.tileExpenditure.Contains(tileType)) return;
        expenditure += TownManager.Instance.tileExpenditure[tileType];
    }

    public virtual void Init()
    {
        Load();
    }
    public virtual void Load() { }
    public Tile GetTile()
    {
        return new Tile(tileType, (int)transform.eulerAngles.y / 90);
    }
    public void DefaultColor()
    {
        ChildsDefaultColor?.Invoke();
    }
    public void PlaceableColor()
    {
        ChildsPlaceableColor?.Invoke();
    }
    public void NotPlaceableColor()
    {
        ChildsNotPlaceableColor?.Invoke();
    }
    public virtual bool CanBePlaced()
    {
        if (!CanPersist()) return false;

        Transform selectedTile = MapHandler.Instance.map[X, Y].transform;
        if (selectedTile == null)
        {
            return false;
        }
        return selectedTile.GetComponent<IMapTile>().GetTile().tileType == TileType.Plains;
    }
    public virtual bool CanPersist()
    {
        return true;
    }
    public virtual void OnRemove()
    {
        for (int dir = 0; dir < 4; dir++)
        {
            GameObject neighbourTile = MapHandler.GetTileFromNeighbour(TilePos, dir * 90);

            if (neighbourTile == null) continue;

            if (!neighbourTile.GetComponent<IMapTile>().CanPersist())
            {
                BuildingSystemHandler.ReplaceTile(new(TileType.Plains, 0), neighbourTile.transform);
            };
        }
    }
}
