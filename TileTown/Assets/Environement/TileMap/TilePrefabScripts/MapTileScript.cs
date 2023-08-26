using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTileScript : MonoBehaviour, IMapTile
{
    [SerializeField] private TileType tile;
    private new Renderer renderer;

    private Color defaultColor;

    private void Start()
    {
        renderer = GetComponent<Renderer>();
        defaultColor = renderer.material.color;
    }
    private void OnMouseEnter()
    {
        renderer.material.color = Color.blue;
    }
    private void OnMouseExit()
    {
        renderer.material.color = defaultColor;
    }

    public TileType GetTileType()
    {
        return tile;
    }
}
