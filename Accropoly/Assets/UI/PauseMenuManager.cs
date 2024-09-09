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
        else // If the game is running
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
        pauseMenu.SetActive(false);
    }
    private void OnPausingGame()
    {
        pauseMenu.SetActive(true);
    }
}