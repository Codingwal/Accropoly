using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button changeMapNameButton;
    [SerializeField] private TMP_InputField mapNameField;
    [SerializeField] private Scrollbar progressBar;

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject loadingScreen;

    private SceneHandler sceneHandler;

    private void Awake()
    {
        sceneHandler = SceneHandler.Instance;

        sceneHandler.LoadingScene += OnLoadingScene;

        startGameButton.onClick.AddListener(OnStartGame);
        changeMapNameButton.onClick.AddListener(OnChangeMapName);


        mainMenu.SetActive(true);
        loadingScreen.SetActive(false);

        mapNameField.text = DataHandler.Instance.GetMapName();
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
