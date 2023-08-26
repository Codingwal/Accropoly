using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] private GameObject buildingMenu;

    [Header("Buttons")]
    [SerializeField] private Button buildingMenuButton;
    [SerializeField] private Button forestButton;
    [SerializeField] private Button streetButton;
    [SerializeField] private Button streetCornerButton;
    [SerializeField] private Button clearButton;

    private BuildingSystemHandler buildingSystemHandler;
    private InputManager inputManager;

    private void Awake()
    {
        buildingSystemHandler = BuildingSystemHandler.Instance;
        inputManager = InputManager.Instance;

        buildingMenuButton.onClick.AddListener(ToggleBuildingMenu);
        forestButton.onClick.AddListener(OnPlaceForest);
        streetButton.onClick.AddListener(OnPlaceStreet);
        streetCornerButton.onClick.AddListener(OnPlaceStreetCorner);
        clearButton.onClick.AddListener(OnClear);
    }
    private void OnEnable()
    {
        inputManager.PriorityEscape += OnEscape;
        inputManager.uIActions.BuildingMenuHotkey.performed += OnBuildingMenuHotkey;
    }
    private void OnDisable()
    {
        inputManager.PriorityEscape -= OnEscape;
        inputManager.uIActions.BuildingMenuHotkey.performed -= OnBuildingMenuHotkey;
    }

    private void OnBuildingMenuHotkey(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        ToggleBuildingMenu();
    }
    private bool OnEscape()
    {
        if (buildingMenu.activeSelf)
        {
            ToggleBuildingMenu();
            return true;
        }
        return false;
    }
    private void ToggleBuildingMenu()
    {
        buildingSystemHandler.highlightTiles = !buildingSystemHandler.highlightTiles;
        buildingMenu.SetActive(buildingSystemHandler.highlightTiles);
    }

    private void OnPlaceForest()
    {
        StartCoroutine(buildingSystemHandler.PlaceTile(TileType.Forest));
    }
    private void OnPlaceStreet()
    {
        StartCoroutine(buildingSystemHandler.PlaceTile(TileType.Street));
    }
    private void OnPlaceStreetCorner()
    {
        StartCoroutine(buildingSystemHandler.PlaceTile(TileType.StreetCorner));
    }
    private void OnClear()
    {
        StartCoroutine(buildingSystemHandler.ClearTile());
    }
}
