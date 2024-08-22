using System;
using System.Collections;
using UnityEngine;

public class BuildingSystemHandler : Singleton<BuildingSystemHandler>
{
    public Transform selectedTile;

    private bool canceled;

    private float tileRotation = 0;

    // private MapHandler mapHandler;
    /*
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
        if (inputManager.inGameActions.Shift.IsPressed())
        {
            tileRotation -= 90;
            if (tileRotation < 0)
            {
                tileRotation += 360;
            }
        }
        else
        {
            tileRotation += 90;
            if (tileRotation > 0)
            {
                tileRotation -= 360;
            }
        }
    }
    public IEnumerator ClearTile()
    {
        // Terminate all other running loops
        yield return TerminatePlacingLoops();

        GameObject tilePrefab = mapHandler.tilePrefabs[TileType.Plains];

        GameObject tile = Instantiate(tilePrefab);

        SetIgnoreRaycast(tile);

        canceled = false;

        IMapTile mapTileScript = tile.GetComponent<IMapTile>();

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

                IMapTile selectedTileScript = selectedTile.GetComponent<IMapTile>();
                mapTileScript.X = selectedTileScript.X;
                mapTileScript.Y = selectedTileScript.Y;

                if (inputManager.inGameActions.Place.IsPressed())
                {
                    ReplaceTile(new(TileType.Plains, 0), selectedTile);
                    tile.transform.eulerAngles = new(tile.transform.eulerAngles.x, tileRotation, tile.transform.eulerAngles.z);

                    SetIgnoreRaycast(tile);

                    mapTileScript = tile.GetComponent<IMapTile>();

                }
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator PlaceTile(TileType tileType)
    {
        yield return TerminatePlacingLoops();

        tileRotation = 0;

        GameObject tilePrefab = mapHandler.tilePrefabs[tileType];

        GameObject tile = Instantiate(tilePrefab);

        SetIgnoreRaycast(tile);

        canceled = false;

        IMapTile mapTileScript = tile.GetComponent<IMapTile>();

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
                tile.transform.eulerAngles = new(tile.transform.eulerAngles.x, tileRotation, tile.transform.eulerAngles.z);

                IMapTile selectedTileScript = selectedTile.GetComponent<IMapTile>();
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
                        ReplaceTile(new(tileType, (int)tile.transform.eulerAngles.y / 90), selectedTile);

                        SetIgnoreRaycast(tile);

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
    }

    private void SetIgnoreRaycast(GameObject tile)
    {
        foreach (Transform child in tile.transform)
        {
            child.gameObject.layer = 2;
        }
    }

    public static void ReplaceTile(Tile newTile, Transform oldTile)
    {
        // Get the old tile script
        IMapTile oldTileScript = oldTile.GetComponent<IMapTile>();

        // If the new tile is the same as the old tile, return
        if (newTile == oldTileScript.GetTile()) return;

        // Buy the tile or return if it can't be bought
        if (!TownManager.Instance.BuyTile(newTile.tileType)) return;

        // Sell the old tile
        TownManager.Instance.SellTile(oldTileScript.GetTile().tileType);

        // Create the new tile
        GameObject newtileObject = Instantiate(MapHandler.Instance.tilePrefabs[newTile.tileType], MapHandler.Instance.tileParent);

        // Get the new tile script
        IMapTile newTileScript = newtileObject.GetComponent<IMapTile>();

        // Set the transform of the new tile
        newtileObject.transform.position = oldTile.position;
        newtileObject.transform.eulerAngles = new(0, newTile.direction * 90);
        newtileObject.transform.SetSiblingIndex(oldTile.GetSiblingIndex());

        // Get the x and y position of the new tile
        newTileScript.X = oldTileScript.X;
        newTileScript.Y = oldTileScript.Y;

        // Save the new tile at that position in the map array
        MapHandler.Instance.map[newTileScript.X, newTileScript.Y] = newtileObject;

        // Initialize the new tile (for example, create inhabitants of a house)
        newTileScript.Init();

        // Call the remove function so the old script can update all neighbours
        oldTileScript.OnRemove();

        // Destroy the old tile
        Destroy(oldTile.gameObject);
    }
    */
}