using System;
using System.Collections;
using UnityEngine;

public class BuildingSystemHandler : Singleton<BuildingSystemHandler>
{
    public bool highlightTiles;
    public Transform selectedTile;

    private bool tilePlaced;
    private bool canceled;

    public GameObject tile;

    private InputManager inputManager;
    private MapHandler mapHandler;
    protected override void Awake()
    {
        base.Awake();

        inputManager = InputManager.Instance;
        mapHandler = MapHandler.Instance;
    }
    private void OnEnable()
    {
        inputManager.inGameActions.Place.performed += OnPlace;
        inputManager.inGameActions.Cancel.performed += OnCancel;
        inputManager.inGameActions.Rotate.performed += OnRotate;
    }
    private void OnDisable()
    {
        inputManager.inGameActions.Place.performed -= OnPlace;
        inputManager.inGameActions.Cancel.performed -= OnCancel;
        inputManager.inGameActions.Rotate.performed -= OnRotate;
    }

    private void OnPlace(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        tilePlaced = true;
    }
    private void OnCancel(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        canceled = true;
    }
    private void OnRotate(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (tilePlaced || canceled)
        {
            return;
        }
        tile.transform.rotation *= Quaternion.Euler(0, 90, 0);
    }

    public IEnumerator ClearTile()
    {
        canceled = false;
        tilePlaced = false;

        while (!(tilePlaced || canceled))
        {
            if (selectedTile != null)
            {
                selectedTile.GetComponent<IMapTile>().PlaceableColor();
            }
            yield return null;
        }
        if (canceled || selectedTile == null)
        {
            yield break;
        }

        GameObject tilePrefab = mapHandler.tilePrefabsDictValues[mapHandler.tilePrefabsDictKeys.IndexOf(TileType.Plains)];
        GameObject tile = Instantiate(tilePrefab);

        tile.transform.position = selectedTile.position;
        tile.transform.parent = MapHandler.Instance.tileParent;
        tile.transform.SetSiblingIndex(selectedTile.GetSiblingIndex());

        Destroy(selectedTile.gameObject);
    }
    public IEnumerator PlaceTile(TileType tileType)
    {
        GameObject tilePrefab = mapHandler.tilePrefabsDictValues[mapHandler.tilePrefabsDictKeys.IndexOf(tileType)];
        tile = Instantiate(tilePrefab);

        // Ignore Raycast
        tile.layer = 2;

        canceled = false;
        tilePlaced = false;

        IMapTile mapTile = null;

        while (!(tilePlaced || canceled))
        {
            if (selectedTile != null)
            {
                tile.SetActive(true);
                tile.transform.position = selectedTile.position + new Vector3(0, 20, 0);

                mapTile = selectedTile.GetComponent<IMapTile>();
                if (mapTile.GetTile().tileType == TileType.Plains)
                {
                    mapTile.PlaceableColor();
                }
                else
                {
                    mapTile.NotPlaceableColor();
                }
            }
            else
            {
                tile.SetActive(false);
            }
            yield return new WaitForEndOfFrame();
        }
        if (canceled || selectedTile == null)
        {
            Destroy(tile);
            yield break;
        }

        if (mapTile.GetTile().tileType == TileType.Plains)
        {
            tile.transform.position -= new Vector3(0, 20, 0);
            tile.transform.parent = MapHandler.Instance.tileParent;
            tile.transform.SetSiblingIndex(selectedTile.GetSiblingIndex());

            Destroy(selectedTile.gameObject);

            // Default layer
            tile.layer = 0;
        }
        else
        {
            Destroy(tile);
        }
    }
}
