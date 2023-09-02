using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;

    [Header("Main menu")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private Button streetMenuButton;
    [SerializeField] private Button buildingsMenuButton;
    [SerializeField] private Button clearButton;

    [Header("Street menu")]
    [SerializeField] private GameObject streetMenu;
    [SerializeField] private Button forestButton;
    [SerializeField] private Button streetButton;
    [SerializeField] private Button streetCornerButton;
    [SerializeField] private Button streetTJunctionButton;
    [SerializeField] private Button streetJunctionButton;
    [SerializeField] private Button emptyButton1;

    [Header("Buildings menu")]
    [SerializeField] private GameObject buildingsMenu;
    [SerializeField] private Button houseButton;
    [SerializeField] private Button skyscraperButton;
    [SerializeField] private Button townhallButton;
    [SerializeField] private Button solarpanelButton;
    [SerializeField] private Button coalPowerPlantButton;
    [SerializeField] private Button emptyButton2;

    private BuildingSystemHandler buildingSystemHandler;
    private InputManager inputManager;

    private void Awake()
    {
        buildingSystemHandler = BuildingSystemHandler.Instance;
        inputManager = InputManager.Instance;

        mainMenuButton.onClick.AddListener(ToggleMainMenu);
        streetMenuButton.onClick.AddListener(ToggleStreetMenu);
        buildingsMenuButton.onClick.AddListener(ToggleBuildingsMenu);

        forestButton.onClick.AddListener(OnPlaceForest);
        streetButton.onClick.AddListener(OnPlaceStreet);
        streetCornerButton.onClick.AddListener(OnPlaceStreetCorner);
        streetTJunctionButton.onClick.AddListener(OnPlaceStreetTJunction);
        streetJunctionButton.onClick.AddListener(OnPlaceStreetJunction);

        houseButton.onClick.AddListener(OnPlaceHouse);
        // skyscraperButton.onClick.AddListener();
        // townhallButton.onClick.AddListener();
        solarpanelButton.onClick.AddListener(OnPlaceSolarPanel);
        coalPowerPlantButton.onClick.AddListener(OnPlaceCoalPowerPlant);

        clearButton.onClick.AddListener(() => { OnClear(new()); });
    }
    private void OnEnable()
    {
        inputManager.PriorityEscape += OnEscape;
        inputManager.MenuToggled += ToggleMainMenu;

        inputManager.MenuHotkey1 += ToggleStreetMenu;
        inputManager.MenuHotkey2 += ToggleBuildingsMenu;
        inputManager.uIActions.Clear.performed += OnClear;

        inputManager.Hotkey1 += OnPlaceForest;
        inputManager.Hotkey2 += OnPlaceStreet;
        inputManager.Hotkey3 += OnPlaceStreetCorner;
        inputManager.Hotkey4 += OnPlaceStreetJunction;
        inputManager.Hotkey5 += OnPlaceStreetTJunction;

        inputManager.Hotkey1 += OnPlaceHouse;
        inputManager.Hotkey4 += OnPlaceSolarPanel;
        inputManager.Hotkey5 += OnPlaceCoalPowerPlant;
    }
    private void OnDisable()
    {
        inputManager.PriorityEscape -= OnEscape;
        inputManager.MenuToggled -= ToggleMainMenu;

        inputManager.MenuHotkey1 -= ToggleStreetMenu;
        inputManager.MenuHotkey2 -= ToggleBuildingsMenu;
        inputManager.uIActions.Clear.performed -= OnClear;

        inputManager.Hotkey1 -= OnPlaceForest;
        inputManager.Hotkey2 -= OnPlaceStreet;
        inputManager.Hotkey3 -= OnPlaceStreetCorner;
        inputManager.Hotkey4 -= OnPlaceStreetJunction;
        inputManager.Hotkey5 -= OnPlaceStreetTJunction;

        inputManager.Hotkey1 -= OnPlaceHouse;
        inputManager.Hotkey4 -= OnPlaceSolarPanel;
        inputManager.Hotkey5 -= OnPlaceCoalPowerPlant;
    }

    private bool OnEscape()
    {
        // if (buildingMenu.activeSelf)
        // {
        //     ToggleBuildingMenu();
        //     return true;
        // }
        // if (mainMenu.activeSelf)
        // {
        //     ToggleMainMenu();
        //     return true;
        // }
        return false;
    }
    private void ToggleMainMenu()
    {
        if (mainMenu.activeSelf)
        {
            mainMenuButton.gameObject.transform.Rotate(0, 0, -90);
            mainMenu.SetActive(false);
        }
        else
        {
            mainMenuButton.gameObject.transform.Rotate(0, 0, 90);
            mainMenu.SetActive(true);
        }

    }
    private void ToggleStreetMenu()
    {
        streetMenu.SetActive(!streetMenu.activeSelf);
    }
    private void ToggleBuildingsMenu()
    {
        buildingsMenu.SetActive(!buildingsMenu.activeSelf);
    }

    private void OnClear(CallbackContext ctx)
    {
        StartCoroutine(buildingSystemHandler.ClearTile());
    }

    private void OnPlaceForest()
    {
        if (!streetMenu.activeSelf) return;
        StartCoroutine(buildingSystemHandler.PlaceTile(TileType.Forest));
    }
    private void OnPlaceStreet()
    {
        if (!streetMenu.activeSelf) return;
        StartCoroutine(buildingSystemHandler.PlaceTile(TileType.Street));
    }
    private void OnPlaceStreetCorner()
    {
        if (!streetMenu.activeSelf) return;
        StartCoroutine(buildingSystemHandler.PlaceTile(TileType.StreetCorner));
    }
    private void OnPlaceStreetTJunction()
    {
        if (!streetMenu.activeSelf) return;
        StartCoroutine(buildingSystemHandler.PlaceTile(TileType.StreetTJunction));
    }
    private void OnPlaceStreetJunction()
    {
        if (!streetMenu.activeSelf) return;
        StartCoroutine(buildingSystemHandler.PlaceTile(TileType.StreetJunction));
    }
    private void OnPlaceHouse()
    {
        if (!buildingsMenu.activeSelf) return;
        StartCoroutine(buildingSystemHandler.PlaceTile(TileType.House));
    }
    private void OnPlaceSolarPanel()
    {
        if (!buildingsMenu.activeSelf) return;
        StartCoroutine(buildingSystemHandler.PlaceTile(TileType.SolarPanel));
    }
    private void OnPlaceCoalPowerPlant()
    {
        if (!buildingsMenu.activeSelf) return;
        StartCoroutine(buildingSystemHandler.PlaceTile(TileType.CoalPowerPlant));
    }
}
