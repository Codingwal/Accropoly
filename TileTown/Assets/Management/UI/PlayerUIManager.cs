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
    [SerializeField] private Button streetTJunctionButton;
    [SerializeField] private Button streetJunctionButton;
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
        streetTJunctionButton.onClick.AddListener(OnPlaceStreetTJunction);
        streetJunctionButton.onClick.AddListener(OnPlaceStreetJunction);

        clearButton.onClick.AddListener(OnClear);
    }
    private void OnEnable()
    {
        inputManager.PriorityEscape += OnEscape;

        inputManager.uIActions.Hotkey1.performed += OnHotkey1;
        inputManager.uIActions.Hotkey2.performed += OnHotkey2;
        inputManager.uIActions.Hotkey3.performed += OnHotkey3;
        inputManager.uIActions.Hotkey4.performed += OnHotkey4;
        inputManager.uIActions.Hotkey5.performed += OnHotkey5;
        inputManager.uIActions.Hotkey6.performed += OnHotkey6;
    }
    private void OnDisable()
    {
        inputManager.PriorityEscape -= OnEscape;

        inputManager.uIActions.Hotkey1.performed -= OnHotkey1;
        inputManager.uIActions.Hotkey2.performed -= OnHotkey2;
        inputManager.uIActions.Hotkey3.performed -= OnHotkey3;
        inputManager.uIActions.Hotkey4.performed -= OnHotkey4;
        inputManager.uIActions.Hotkey5.performed -= OnHotkey5;
        inputManager.uIActions.Hotkey6.performed -= OnHotkey6;
    }

    private void OnHotkey1(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (inputManager.inGameActions.Shift.IsPressed())
        {
            ToggleBuildingMenu();
        }
        else
        {
            OnPlaceForest();
        }

    }
    private void OnHotkey2(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        OnPlaceStreet();
    }
    private void OnHotkey3(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        OnPlaceStreetCorner();
    }
    private void OnHotkey4(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        OnPlaceStreetTJunction();
    }
    private void OnHotkey5(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        OnPlaceStreetJunction();
    }
    private void OnHotkey6(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        OnClear();
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
    private void OnPlaceStreetTJunction()
    {
        StartCoroutine(buildingSystemHandler.PlaceTile(TileType.StreetTJunction));
    }
    private void OnPlaceStreetJunction()
    {
        StartCoroutine(buildingSystemHandler.PlaceTile(TileType.StreetJunction));
    }
    private void OnClear()
    {
        StartCoroutine(buildingSystemHandler.ClearTile());
    }
}
