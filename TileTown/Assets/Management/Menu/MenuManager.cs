using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject loadingScreen;


    [SerializeField] private Button startGameButton;

    [SerializeField] private Button createMapButton;
    [SerializeField] private TMP_Dropdown mapTemplateDropdown;

    [SerializeField] private Button changeMapNameButton;
    [SerializeField] private TMP_InputField mapNameField;


    [SerializeField] private Scrollbar progressBar;

    private SceneHandler sceneHandler;

    private void Awake()
    {
        sceneHandler = SceneHandler.Instance;

        sceneHandler.LoadingScene += OnLoadingScene;

        startGameButton.onClick.AddListener(OnStartGame);
        createMapButton.onClick.AddListener(OnCreateMap);
        changeMapNameButton.onClick.AddListener(OnChangeMapName);


        mainMenu.SetActive(true);
        loadingScreen.SetActive(false);

        mapNameField.text = DataHandler.Instance.GetMapName();

        string[] mapTemplates = FileHandler.ListFiles("Templates");

        mapTemplateDropdown.options = new();
        foreach (string template in mapTemplates)
        {
            mapTemplateDropdown.options.Add(new(template));
        }
    }
    private void OnCreateMap()
    {
        string mapTemplateName = mapTemplateDropdown.options[mapTemplateDropdown.value].text;
        Serializable2DArray<TileType> mapTemplate = FileHandler.LoadObject<Serializable2DArray<TileType>>("Templates", mapTemplateName);
        FileHandler.SaveObject("Saves", mapNameField.text, mapTemplate);
    }
    private void OnLoadingScene(float progress)
    {
        progressBar.value = progress;
    }
    private void OnStartGame()
    {
        mainMenu.SetActive(false);
        loadingScreen.SetActive(true);

        GameLoopManager.Instance.GameState = GameState.InGame;
    }
    private void OnChangeMapName()
    {
        DataHandler.Instance.ChangeMapName(mapNameField.text);
    }
}
