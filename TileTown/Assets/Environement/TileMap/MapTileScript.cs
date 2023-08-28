using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTileScript : MonoBehaviour, IMapTile
{
    [SerializeField] private TileType tileType;
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
        // if (buildingSystemHandler.tile == gameObject)
        // {
        //     return;
        // }
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
}
