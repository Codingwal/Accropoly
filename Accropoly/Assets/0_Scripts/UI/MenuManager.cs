using TMPro;
using UnityEngine;
using UnityEngine.UI;

using UIAction = UIInputData.Action;

public class MenuManager : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject pauseMenu;


    [Header("Main menu")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button createMapButton;
    [SerializeField] private TMP_InputField mapNameField;
    [SerializeField] private TMP_Dropdown mapTemplateDropdown;
    [SerializeField] private Button deleteMapButton;
    [SerializeField] private TMP_Dropdown mapsDropdown;


    [Header("Pause menu")]
    [SerializeField] private Button toMainMenuButton;
    [SerializeField] private Button continueButton;

    private string SelectedWorldName => mapsDropdown.options[mapsDropdown.value].text;
    private string SelectedMapTemplateName => mapTemplateDropdown.options[mapTemplateDropdown.value].text;
    private void Awake()
    {
        InputSystem.uiInput += OnUIInput;

        startGameButton.onClick.AddListener(OnStartGame);
        createMapButton.onClick.AddListener(OnCreateMap);
        deleteMapButton.onClick.AddListener(OnDeleteMap);

        continueButton.onClick.AddListener(() => MenuUtility.ContinueGame());
        toMainMenuButton.onClick.AddListener(() =>
        {
            MenuUtility.QuitGame();
            mainMenu.SetActive(true);
            pauseMenu.SetActive(false);
        });
        MenuUtility.continuingGame += () => pauseMenu.SetActive(false);
        MenuUtility.pausingGame += () => pauseMenu.SetActive(true);

        mainMenu.SetActive(true);
        pauseMenu.SetActive(false);

        mapNameField.text = SaveSystem.Instance.GetWorldName();

        ReloadUI();
    }
    private void OnDisable()
    {
        InputSystem.uiInput -= OnUIInput;
    }

    private void OnStartGame()
    {
        mainMenu.SetActive(false);

        MenuUtility.StartGame(SelectedWorldName);
    }
    private void OnCreateMap()
    {
        MenuUtility.CreateWorld(mapNameField.text, SelectedMapTemplateName);
        ReloadUI();
    }
    private void OnDeleteMap()
    {
        MenuUtility.DeleteWorld(SelectedWorldName);
        ReloadUI();
    }
    private void OnUIInput(UIInputData inputData)
    {
        if (inputData.action == UIAction.Escape)
            if (pauseMenu.activeSelf) // If the game is paused
                MenuUtility.ContinueGame();
            else // If the game is running
                MenuUtility.PauseGame();
        else if (inputData.action == UIAction.Clear)
            MenuUtility.PlaceTile(TileType.Plains);
        else if (inputData.action == UIAction.Hotkey)
        {
            MenuUtility.PlaceTile(inputData.hotkey switch
            {
                1 => TileType.Sapling,
                2 => TileType.House,
                3 => TileType.SolarPanel,
                4 => TileType.Sapling,
                5 => TileType.Sapling,
                6 => TileType.Sapling,
                7 => TileType.Sapling,
                _ => throw new()
            });
        }
    }
    private void ReloadUI()
    {
        string[] mapTemplates = MenuUtility.GetMapTemplateNames();
        mapTemplateDropdown.options = new();
        foreach (string template in mapTemplates)
        {
            mapTemplateDropdown.options.Add(new(template));
        }
        mapTemplateDropdown.value = 0;
        mapTemplateDropdown.RefreshShownValue();

        string[] maps = MenuUtility.GetWorldNames();
        mapsDropdown.options = new();
        foreach (string map in maps)
        {
            mapsDropdown.options.Add(new(map));
        }
        mapsDropdown.value = 0;
        mapsDropdown.RefreshShownValue();
    }
}
