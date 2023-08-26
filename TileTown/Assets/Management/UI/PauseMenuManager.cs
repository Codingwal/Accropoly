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
        gameLoopManager.GameStateChanged += OnGameStateChanged;

        continueButton.onClick.AddListener(OnContinuePressed);
        toMainMenuButton.onClick.AddListener(OnToMainMenuPressed);
    }
    private void OnDisable()
    {
        gameLoopManager.GameStateChanged -= OnGameStateChanged;
    }

    private void OnContinuePressed()
    {
        gameLoopManager.GameState = GameState.InGame;
    }

    private void OnToMainMenuPressed()
    {
        gameLoopManager.GameState = GameState.MainMenu;
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