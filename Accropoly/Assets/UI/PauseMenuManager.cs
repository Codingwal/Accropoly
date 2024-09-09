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

    private void Awake()
    {
        continueButton.onClick.AddListener(OnContinuePressed);
        toMainMenuButton.onClick.AddListener(OnToMainMenuPressed);

        MenuUtility.continuingGame += OnContinuingGame;
        MenuUtility.pausingGame += OnPausingGame;

        InputSystem.escape += OnEscape;

    }
    private void OnEscape()
    {
        if (pauseMenu.activeSelf) // If the game is paused
            MenuUtility.ContinueGame();
        else
            MenuUtility.PauseGame();
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