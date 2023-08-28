using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTileScript : MonoBehaviour, IMapTile
{
    [SerializeField] private TileType tileType;

    // Index
    public int X { get; set; }
    public int Y { get; set; }


    private Color defaultColor;

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
        Transform selectedTile = MapHandler.Instance.map[X, Y].transform;
        if (selectedTile == null)
        {
            return false;
        }
        return selectedTile.GetComponent<IMapTile>().GetTile().tileType == TileType.Plains;
    }
}
