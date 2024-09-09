using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private Button toMainMenuButton;
    [SerializeField] private Button continueButton;

    private GameLoopManager gameLoopManager;
    private void Awake()
    {
        gameLoopManager = GameLoopManager.Instance;

        continueButton.onClick.AddListener(OnContinuePressed);
        toMainMenuButton.onClick.AddListener(OnToMainMenuPressed);

        MenuUtility.continuingGame += OnContinuingGame;
        MenuUtility.pausingGame += OnPausingGame;

        InputManager.Escape += OnEscape;

    }
    private void OnEscape()
    {
        switch (gameLoopManager.GameState)
        {
            case GameState.PauseMenu:
                MenuUtility.ContinueGame();
                break;
            case GameState.InGame:
                MenuUtility.PauseGame();
                break;
        }
    }
    private void OnContinuePressed()
    {
        MenuUtility.ContinueGame();
    }

    private void OnToMainMenuPressed()
    {
        // TODO: Activate MainMenu
    }

    private void OnContinuingGame()
    {

    }
    private void OnPausingGame()
    {

    }
    private void OnGameStateChanged(GameState newGameState, GameState oldGameState)
    {
        if (pauseMenu == null)
        {
            pauseMenu = GameObject.Find("PauseMenu");
        }
        switch (newGameState)
        {
            case GameState.InGame:
                pauseMenu.SetActive(false);
                break;
            case GameState.PauseMenu:
                pauseMenu.SetActive(true);
                break;
        }

    }
}