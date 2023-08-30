using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;

    [Header("Main menu")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private Button buildingMenuButton;
    [SerializeField] private Button clearButton;

    [Header("Building menu")]
    [SerializeField] private GameObject buildingMenu;
    [SerializeField] private Button forestButton;
    [SerializeField] private Button streetButton;
    [SerializeField] private Button streetCornerButton;
    [SerializeField] private Button streetTJunctionButton;
    [SerializeField] private Button streetJunctionButton;
    [SerializeField] private Button houseButton;

    private BuildingSystemHandler buildingSystemHandler;
    private InputManager inputManager;

    private void Awake()
    {
        buildingSystemHandler = BuildingSystemHandler.Instance;
        inputManager = InputManager.Instance;

        mainMenuButton.onClick.AddListener(ToggleMainMenu);
        buildingMenuButton.onClick.AddListener(ToggleBuildingMenu);

        forestButton.onClick.AddListener(OnPlaceForest);
        streetButton.onClick.AddListener(OnPlaceStreet);
        streetCornerButton.onClick.AddListener(OnPlaceStreetCorner);
        streetTJunctionButton.onClick.AddListener(OnPlaceStreetTJunction);
        streetJunctionButton.onClick.AddListener(OnPlaceStreetJunction);
        houseButton.onClick.AddListener(OnPlaceHouse);

        clearButton.onClick.AddListener(() => { OnClear(new()); });
    }
    private void OnEnable()
    {
        inputManager.PriorityEscape += OnEscape;
        inputManager.MenuToggled += ToggleMainMenu;

        inputManager.MenuHotkey1 += ToggleBuildingMenu;
        inputManager.uIActions.Clear.performed += OnClear;

        inputManager.Hotkey1 += OnPlaceForest;
        inputManager.Hotkey2 += OnPlaceStreet;
        inputManager.Hotkey3 += OnPlaceStreetCorner;
        inputManager.Hotkey4 += OnPlaceStreetJunction;
        inputManager.Hotkey5 += OnPlaceStreetTJunction;
        inputManager.Hotkey6 += OnPlaceHouse;
    }
    private void OnDisable()
    {
        inputManager.PriorityEscape -= OnEscape;
        inputManager.MenuToggled -= ToggleMainMenu;

        inputManager.MenuHotkey1 -= ToggleBuildingMenu;
        inputManager.uIActions.Clear.performed -= OnClear;

        inputManager.Hotkey1 -= OnPlaceForest;
        inputManager.Hotkey2 -= OnPlaceStreet;
        inputManager.Hotkey3 -= OnPlaceStreetCorner;
        inputManager.Hotkey4 -= OnPlaceStreetJunction;
        inputManager.Hotkey5 -= OnPlaceStreetTJunction;
        inputManager.Hotkey6 -= OnPlaceHouse;
    }

    private bool OnEscape()
    {
        if (buildingMenu.activeSelf)
        {
            ToggleBuildingMenu();
            return true;
        }
        if (mainMenu.activeSelf)
        {
            ToggleMainMenu();
            return true;
        }
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
    private void ToggleBuildingMenu()
    {
        buildingMenu.SetActive(!buildingMenu.activeSelf);
    }

    private void OnClear(CallbackContext ctx)
    {
        StartCoroutine(buildingSystemHandler.ClearTile());
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
    private void OnPlaceHouse()
    {
        StartCoroutine(buildingSystemHandler.PlaceTile(TileType.House));
    }
}
