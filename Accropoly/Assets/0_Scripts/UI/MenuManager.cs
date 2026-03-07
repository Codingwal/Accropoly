using TMPro;
using UnityEngine;
using UnityEngine.UI;

using UIAction = Components.UIInputData.Action;

public class MenuManager : MonoBehaviour
{

    [Header("Menus")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject optionMenu;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject statisticsDisplay;


    [Header("Main menu")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private TMP_Dropdown mapsDropdown;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;


    [Header("Option menu")]
    [SerializeField] private TextMeshProUGUI selectedMap;
    [SerializeField] private Button createMapButton;
    [SerializeField] private TMP_InputField mapNameField;
    [SerializeField] private TMP_Dropdown mapTemplateDropdown;
    [SerializeField] private string defaultTemplate;
    [SerializeField] private Button createTemplateButton;
    [SerializeField] private Button createStandardTemplatesButton;
    [SerializeField] private Button deleteMapButton;
    [SerializeField] private Button showInExplorerButton;
    [SerializeField] private Button backToMainMenuButton;


    [Header("Pause menu")]
    [SerializeField] private Button toMainMenuButton;
    [SerializeField] private Button continueButton;

    private string SelectedWorldName => mapsDropdown.options.Count > 0 ? mapsDropdown.options[mapsDropdown.value].text : "None";
    private string SelectedMapTemplateName => mapTemplateDropdown.options.Count > 0 ? mapTemplateDropdown.options[mapTemplateDropdown.value].text : "None";
    private void Awake()
    {
        InputHandler.uiInput += OnUIInput;

        startGameButton.onClick.AddListener(OnStartGame);
        quitButton.onClick.AddListener(OnQuit);
        optionsButton.onClick.AddListener(OnOptions);

        createMapButton.onClick.AddListener(OnCreateMap);
        createTemplateButton.onClick.AddListener(OnCreateTemplate);
        createStandardTemplatesButton.onClick.AddListener(OnCreateStandardTemplates);
        deleteMapButton.onClick.AddListener(OnDeleteMap);
        showInExplorerButton.onClick.AddListener(OnShowInExplorer);
        backToMainMenuButton.onClick.AddListener(OnBackToMainMenu);

        continueButton.onClick.AddListener(() => MenuUtility.ContinueGame());
        toMainMenuButton.onClick.AddListener(() =>
        {
            mainMenu.SetActive(true);
            optionMenu.SetActive(false);
            pauseMenu.SetActive(false);
            MenuUtility.QuitGame();
        });

        MenuUtility.continuingGame += () =>
        {
            pauseMenu.SetActive(false);
            statisticsDisplay.SetActive(true);
        };
        MenuUtility.pausingGame += () =>
        {
            pauseMenu.SetActive(true);
            statisticsDisplay.SetActive(false);
        };

        mainMenu.SetActive(true);
        optionMenu.SetActive(false);
        pauseMenu.SetActive(false);

        //mapNameField.text = SaveSystem.Instance.GetWorldName();

        ReloadUI();
    }
    private void OnDisable()
    {
        InputHandler.uiInput -= OnUIInput;
    }

    private void OnStartGame()
    {
        MenuUtility.StartGame(SelectedWorldName);

        mainMenu.SetActive(false);
    }
    private void OnOptions()
    {
        mainMenu.SetActive(false);
        optionMenu.SetActive(true);
        selectedMap.text = "Selected map: " + SelectedWorldName;
    }
    private void OnQuit()
    {
        MenuUtility.Quit();
    }

    private void OnCreateMap()
    {
        optionMenu.SetActive(false);
        ReloadUI();

        MenuUtility.CreateWorld(mapNameField.text, SelectedMapTemplateName);

        MenuUtility.StartGame(mapNameField.text);
    }
    private void OnCreateTemplate()
    {
        if (mapsDropdown.options.Count == 0) return;
        MenuUtility.CreateTemplate(SelectedWorldName, SelectedWorldName); // to make it easier, the template name is the same as the map name
        ReloadUI();
    }
    private void OnCreateStandardTemplates()
    {
        var templates = MapTemplates.mapTemplates;
        foreach (var pair in templates)
        {
            SaveSystem.Instance.SaveTemplate(pair.Value, pair.Key.ToString());
        }
        ReloadUI();
    }
    private void OnDeleteMap()
    {
        if (mapsDropdown.options.Count == 0) return;
        MenuUtility.DeleteWorld(SelectedWorldName);
        ReloadUI();
    }
    private void OnShowInExplorer()
    {
        MenuUtility.OpenExplorer();
    }

    private void OnBackToMainMenu()
    {
        mainMenu.SetActive(true);
        optionMenu.SetActive(false);
    }

    private void OnUIInput(Components.UIInputData inputData)
    {
        switch (inputData.action)
        {
            case UIAction.Escape:
                if (pauseMenu.activeSelf) // If the game is paused       
                    MenuUtility.ContinueGame();
                else // If the game is running         
                    MenuUtility.PauseGame();

                break;
            case UIAction.HideUI:
                if (statisticsDisplay.activeSelf)
                    statisticsDisplay.SetActive(false);
                else
                    statisticsDisplay.SetActive(true);
                break;
            case UIAction.Clear:
                MenuUtility.PlaceTile(TileType.Plains);
                break;
            case UIAction.Hotkey:
                MenuUtility.PlaceTile(inputData.hotkey switch
                {
                    1 => TileType.Sapling,
                    2 => TileType.House,
                    3 => TileType.SolarPanel,
                    4 => TileType.Street,
                    5 => TileType.River,
                    6 => TileType.Lake,
                    7 => TileType.Office,
                    8 => TileType.WindTurbine,
                    9 => TileType.Bitumen,
                    _ => throw new()
                });
                break;
        }
    }
    private void ReloadUI()
    {
        string[] mapTemplates = MenuUtility.GetMapTemplateNames();
        mapTemplateDropdown.options = new();
        foreach (string template in mapTemplates)
        {
            mapTemplateDropdown.options.Add(new(template));

            // Plains as default template for convinience
            if (template == defaultTemplate) mapTemplateDropdown.value = mapTemplateDropdown.options.Count;
        }
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
