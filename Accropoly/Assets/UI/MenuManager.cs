using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject mainMenu;
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

    private void Awake()
    {
        SceneManagement.LoadingScene += OnLoadingScene;

        startGameButton.onClick.AddListener(OnStartGame);
        createMapButton.onClick.AddListener(OnCreateMap);
        deleteMapButton.onClick.AddListener(OnDeleteMap);

        mainMenu.SetActive(true);
        loadingScreen.SetActive(false);

        mapNameField.text = FileHandler.GetWorldName();

        ReloadUI();
    }

    private void OnStartGame()
    {
        mainMenu.SetActive(false);
        loadingScreen.SetActive(true);

        FileHandler.ChangeWorldName(mapsDropdown.options[mapsDropdown.value].text);

        _ = SceneManagement.LoadScene("Game");  // Discard used to prevent compiler warning
    }
    private void OnCreateMap()
    {
        FileHandler.ChangeWorldName(mapNameField.text);

        string mapTemplateName = mapTemplateDropdown.options[mapTemplateDropdown.value].text;

        FileHandler.CreateWorld(mapTemplateName);


        ReloadUI();
    }
    private void OnDeleteMap()
    {
        FileHandler.DeleteFile("Saves", mapsDropdown.options[mapsDropdown.value].text);
        ReloadUI();
    }

    private void OnLoadingScene(float progress)
    {
        progressBar.value = progress;
    }

    private void ReloadUI()
    {
        string[] mapTemplates = FileHandler.ListFiles("Templates");
        mapTemplateDropdown.options = new();
        foreach (string template in mapTemplates)
        {
            mapTemplateDropdown.options.Add(new(template));
        }
        // mapTemplateDropdown.value = 0;
        mapTemplateDropdown.RefreshShownValue();

        string[] maps = FileHandler.ListFiles("Saves");
        mapsDropdown.options = new();
        foreach (string map in maps)
        {
            mapsDropdown.options.Add(new(map));
        }
        // mapsDropdown.value = 0;
        mapTemplateDropdown.RefreshShownValue();
    }
}
