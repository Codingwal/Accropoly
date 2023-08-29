using System;
using System.Collections;
using UnityEngine;

public class BuildingSystemHandler : Singleton<BuildingSystemHandler>
{
    public Transform selectedTile;

    private bool canceled;

    public GameObject tile = null;
    private IMapTile mapTileScript;
    private InputManager inputManager;
    private MapHandler mapHandler;

    IMapTile selectedTileScript;
    protected override void Awake()
    {
        base.Awake();
    }
    private void OnEnable()
    {
        inputManager = InputManager.Instance;
        mapHandler = MapHandler.Instance;

        inputManager.inGameActions.Cancel.performed += OnCancel;
        inputManager.inGameActions.Rotate.performed += OnRotate;
    }
    private void OnDisable()
    {
        inputManager.inGameActions.Cancel.performed -= OnCancel;
        inputManager.inGameActions.Rotate.performed -= OnRotate;
    }

    private void OnCancel(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        canceled = true;
    }
    private void OnRotate(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (tile.transform == null)
        {
            return;
        }
        if (inputManager.inGameActions.Shift.IsPressed())
        {
            tile.transform.rotation *= Quaternion.Euler(0, -90, 0);
        }
        else
        {
            tile.transform.rotation *= Quaternion.Euler(0, 90, 0);
        }

    }
    public IEnumerator ClearTile()
    {
        // Terminate all other running loops
        yield return TerminatePlacingLoops();

        GameObject tilePrefab = mapHandler.tilePrefabsDictValues[mapHandler.tilePrefabsDictKeys.IndexOf(TileType.Plains)];

        tile = Instantiate(tilePrefab);

        tile.layer = 2;

        canceled = false;

        mapTileScript = tile.GetComponent<IMapTile>();

        while (true)
        {
            if (canceled)
            {
                Destroy(tile);
                yield break;
            }
            if (selectedTile == null)
            {
                tile.SetActive(false);
            }
            else
            {
                tile.SetActive(true);
                tile.transform.position = selectedTile.position + new Vector3(0, 1, 0);
                mapTileScript.PlaceableColor();

                selectedTileScript = selectedTile.GetComponent<IMapTile>();
                mapTileScript.X = selectedTileScript.X;
                mapTileScript.Y = selectedTileScript.Y;

                if (inputManager.inGameActions.Place.IsPressed())
                {

                    Quaternion tileRotation = tile.transform.rotation;
                    // ReplaceTile();

                    tile = Instantiate(tilePrefab);
                    tile.transform.rotation = tileRotation;
                    tile.layer = 2;

                    mapTileScript = tile.GetComponent<IMapTile>();

                }
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator PlaceTile(TileType tileType)
    {
        yield return TerminatePlacingLoops();

        GameObject tilePrefab = mapHandler.tilePrefabsDictValues[mapHandler.tilePrefabsDictKeys.IndexOf(tileType)];

        tile = Instantiate(tilePrefab);
        tile.layer = 2;

        canceled = false;

        mapTileScript = tile.GetComponent<IMapTile>();

        while (true)
        {
            if (canceled)
            {
                Destroy(tile);
                yield break;
            }

            if (selectedTile == null)
            {
                tile.SetActive(false);
            }
            else
            {
                tile.SetActive(true);
                tile.transform.position = selectedTile.position + new Vector3(0, 1, 0);

                selectedTileScript = selectedTile.GetComponent<IMapTile>();
                mapTileScript.X = selectedTileScript.X;
                mapTileScript.Y = selectedTileScript.Y;

                if (mapTileScript.CanBePlaced())
                {
                    mapTileScript.PlaceableColor();
                }
                else
                {
                    mapTileScript.NotPlaceableColor();
                }
                if (inputManager.inGameActions.Place.IsPressed())
                {
                    if (mapTileScript.CanBePlaced())
                    {
                        Quaternion tileRotation = tile.transform.rotation;
                        // ReplaceTile();

                        tile = Instantiate(tilePrefab);
                        tile.transform.rotation = tileRotation;
                        tile.layer = 2;

                        mapTileScript = tile.GetComponent<IMapTile>();
                    }
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator TerminatePlacingLoops()
    {
        canceled = true;
        yield return new WaitForEndOfFrame();
        if (tile != null)
        {
            Destroy(tile);
        }
    }
    private void ReplaceTile(Tile newTile, GameObject oldTile)
    {
        // tile = Instantiate(MapHandler.Instance.)
        tile.transform.position = selectedTile.position;
        tile.transform.parent = MapHandler.Instance.tileParent;
        tile.transform.SetSiblingIndex(selectedTile.GetSiblingIndex());

        mapTileScript.X = selectedTileScript.X;
        mapTileScript.Y = selectedTileScript.Y;
        MapHandler.Instance.map[mapTileScript.X, mapTileScript.Y] = tile;

        tile.layer = 0;
        mapTileScript.DefaultColor();

        Destroy(selectedTile.gameObject);
    }
}