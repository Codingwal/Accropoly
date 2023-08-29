using System;
using System.Collections;
using UnityEngine;

public class BuildingSystemHandler : Singleton<BuildingSystemHandler>
{
    public Transform selectedTile;

    private bool canceled;

    private float tileRotation = 0;

    private InputManager inputManager;
    private MapHandler mapHandler;
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

        tile.layer = 2;

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

        tileRotation = 0;

        GameObject tilePrefab = mapHandler.tilePrefabs[tileType];

        GameObject tile = Instantiate(tilePrefab);
        tile.layer = 2;

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
    }
    public static void ReplaceTile(Tile newTile, Transform oldTile)
    {
        GameObject newtileObject = Instantiate(MapHandler.Instance.tilePrefabs[newTile.tileType], MapHandler.Instance.tileParent);

        newtileObject.transform.position = oldTile.position;
        newtileObject.transform.eulerAngles = new(0, newTile.direction * 90);
        newtileObject.transform.SetSiblingIndex(oldTile.GetSiblingIndex());

        IMapTile oldTileScript = oldTile.GetComponent<IMapTile>();
        IMapTile newTileScript = newtileObject.GetComponent<IMapTile>();

        newTileScript.X = oldTileScript.X;
        newTileScript.Y = oldTileScript.Y;

        MapHandler.Instance.map[newTileScript.X, newTileScript.Y] = newtileObject;

        newtileObject.layer = 0;
        newTileScript.DefaultColor();

        Destroy(oldTile.gameObject);
    }
}