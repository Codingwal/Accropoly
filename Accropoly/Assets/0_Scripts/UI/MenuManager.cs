using TMPro;
using UnityEngine;
using UnityEngine.UI;

using UIAction = Components.UIInputData.Action;

public class MenuManager : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject statisticsDisplay;


    [Header("Main menu")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button createMapButton;
    [SerializeField] private TMP_InputField mapNameField;
    [SerializeField] private TMP_Dropdown mapTemplateDropdown;
    [SerializeField] private string defaultTemplate;
    [SerializeField] private Button createTemplateButton;
    [SerializeField] private Button deleteMapButton;
    [SerializeField] private TMP_Dropdown mapsDropdown;
    [SerializeField] private Button showInExplorerButton;
    [SerializeField] private Button quitButton;


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
        createTemplateButton.onClick.AddListener(OnCreateTemplate);
        deleteMapButton.onClick.AddListener(OnDeleteMap);
        showInExplorerButton.onClick.AddListener(OnShowInExplorer);
        quitButton.onClick.AddListener(OnQuit);

        continueButton.onClick.AddListener(() => MenuUtility.ContinueGame());
        toMainMenuButton.onClick.AddListener(() =>
        {
            MenuUtility.QuitGame();
            mainMenu.SetActive(true);
            pauseMenu.SetActive(false);
        });
        MenuUtility.continuingGame += () => { pauseMenu.SetActive(false); statisticsDisplay.SetActive(true); };
        MenuUtility.pausingGame += () => { pauseMenu.SetActive(true); statisticsDisplay.SetActive(false); };

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

        // Select the newly created world for convinience
        mapsDropdown.value = mapsDropdown.options.FindIndex(x => x.text == mapNameField.text);
    }
    private void OnCreateTemplate()
    {
        if (mapsDropdown.options.Count == 0) return;
        MenuUtility.CreateTemplate(SelectedWorldName, mapNameField.text);
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
    private void OnQuit()
    {
        MenuUtility.Quit();
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
                    6 => TileType.Hut,
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
