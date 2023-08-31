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

    // Design
    private Color defaultColor;

    // Inhabitants
    private List<GameObject> inhabitants = new();

    // References
    private new Renderer renderer;
    private BuildingSystemHandler buildingSystemHandler;


    private void Awake()
    {
        renderer = GetComponent<Renderer>();
        buildingSystemHandler = BuildingSystemHandler.Instance;

        defaultColor = renderer.material.color;
    }
    private void OnMouseEnter()
    {
        if (this == null)
        {
            return;
        }
        buildingSystemHandler.selectedTile = transform;
    }
    private void OnMouseExit()
    {
        DefaultColor();

        if (buildingSystemHandler.selectedTile == transform)
        {
            buildingSystemHandler.selectedTile = null;
        }
    }

    public void Init()
    {
        if (tileType == TileType.House)
        {
            inhabitants = PopulationManager.Instance.NewHouse(HouseSize.normal, TilePos);
        }
    }

    public Tile GetTile()
    {
        return new Tile(tileType, (int)transform.eulerAngles.y / 90);
    }
    public void DefaultColor()
    {
        renderer.material.color = defaultColor;
    }
    public void PlaceableColor()
    {
        renderer.material.color = Color.green;
    }
    public void NotPlaceableColor()
    {
        renderer.material.color = Color.red;
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
    public void OnRemove()
    {
        for (int i = 0; i < inhabitants.Count; i++)
        {
            PopulationManager.Instance.RemovePerson(inhabitants[i]);
        }

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
