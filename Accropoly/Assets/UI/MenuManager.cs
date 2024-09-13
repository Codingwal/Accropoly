using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        InputSystem.escape += OnEscape;

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
        InputSystem.escape -= OnEscape;
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
