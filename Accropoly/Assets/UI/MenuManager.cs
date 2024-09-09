using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject loadingScreen;


    [Header("Main menu")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button createMapButton;
    [SerializeField] private TMP_InputField mapNameField;
    [SerializeField] private TMP_Dropdown mapTemplateDropdown;
    [SerializeField] private Button deleteMapButton;
    [SerializeField] private TMP_Dropdown mapsDropdown;

    [Header("Loading screen")]
    [SerializeField] private Scrollbar progressBar;


    [Header("Pause menu")]
    [SerializeField] private Button toMainMenuButton;
    [SerializeField] private Button continueButton;

    private string SelectedWorldName => mapsDropdown.options[mapsDropdown.value].text;
    private void Awake()
    {
        MenuUtility.loadingWorld += OnLoadingWorld;

        startGameButton.onClick.AddListener(OnStartGame);
        createMapButton.onClick.AddListener(OnCreateMap);
        deleteMapButton.onClick.AddListener(OnDeleteMap);

        mainMenu.SetActive(true);
        loadingScreen.SetActive(false);

        mapNameField.text = SaveSystem.Instance.GetWorldName();

        continueButton.onClick.AddListener(() => MenuUtility.ContinueGame());
        toMainMenuButton.onClick.AddListener(() => { }); // TODO: Activate MainMenu

        MenuUtility.continuingGame += () => pauseMenu.SetActive(false);
        MenuUtility.pausingGame += () => pauseMenu.SetActive(true);

        InputSystem.escape += OnEscape;

        ReloadUI();
    }

    private void OnStartGame()
    {
        mainMenu.SetActive(false);
        loadingScreen.SetActive(true);

        MenuUtility.LoadWorld(SelectedWorldName);
    }
    private void OnCreateMap()
    {
        MenuUtility.CreateWorld(mapNameField.text, SelectedWorldName);
        ReloadUI();
    }
    private void OnDeleteMap()
    {
        MenuUtility.DeleteWorld(SelectedWorldName);
        ReloadUI();
    }

    private void OnLoadingWorld(float progress)
    {
        progressBar.value = progress;
    }
    private void OnEscape()
    {
        if (pauseMenu.activeSelf) // If the game is paused
            MenuUtility.ContinueGame();
        else // If the game is running
            MenuUtility.PauseGame();
    }
    private void ReloadUI()
    {
        string[] mapTemplates = MenuUtility.GetMapTemplateNames();
        mapTemplateDropdown.options = new();
        foreach (string template in mapTemplates)
        {
            mapTemplateDropdown.options.Add(new(template));
        }
        // mapTemplateDropdown.value = 0;
        mapTemplateDropdown.RefreshShownValue();

        string[] maps = MenuUtility.GetWorldNames();
        mapsDropdown.options = new();
        foreach (string map in maps)
        {
            mapsDropdown.options.Add(new(map));
        }
        // mapsDropdown.value = 0;
        mapTemplateDropdown.RefreshShownValue();
    }
}
