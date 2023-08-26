using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTileScript : MonoBehaviour, IMapTile
{
    [SerializeField] private TileType tileType;
    private Color defaultColor;

    private new Renderer renderer;
    private BuildingSystemHandler buildingSystemHandler;


    private void Start()
    {
        renderer = GetComponent<Renderer>();
        buildingSystemHandler = BuildingSystemHandler.Instance;

        defaultColor = renderer.material.color;
    }
    private void OnMouseEnter()
    {
        if (buildingSystemHandler.highlightTiles)
        {
            renderer.material.color = Color.blue;

            buildingSystemHandler.selectedTile = transform;
        }
    }
    private void OnMouseExit()
    {
        renderer.material.color = defaultColor;
    }

    public Tile GetTile()
    {
        return new Tile(tileType, (int)transform.eulerAngles.y / 90);
    }
}
