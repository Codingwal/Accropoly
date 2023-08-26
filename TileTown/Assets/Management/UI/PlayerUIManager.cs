using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] private GameObject buildingMenu;

    [Header("Buttons")]
    [SerializeField] private Button buildingMenuButton;
    [SerializeField] private Button forestButton;
    [SerializeField] private Button streetButton;
    [SerializeField] private Button streetCornerButton;
    [SerializeField] private Button clearButton;

    private BuildingSystemHandler buildingSystemHandler;
    private InputManager inputManager;

    private void Awake()
    {
        buildingSystemHandler = BuildingSystemHandler.Instance;
        inputManager = InputManager.Instance;

        buildingMenuButton.onClick.AddListener(ToggleBuildingMenu);
    }
    private void OnEnable()
    {
        inputManager.PriorityEscape += OnEscape;
        inputManager.uIActions.BuildingMenuHotkey.performed += OnBuildingMenuHotkey;
    }
    private void OnDisable()
    {
        inputManager.PriorityEscape -= OnEscape;
        inputManager.uIActions.BuildingMenuHotkey.performed -= OnBuildingMenuHotkey;
    }

    private void OnBuildingMenuHotkey(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        ToggleBuildingMenu();
    }
    private bool OnEscape()
    {
        if (buildingMenu.activeSelf)
        {
            ToggleBuildingMenu();
            return true;
        }
        return false;
    }

    private void ToggleBuildingMenu()
    {
        buildingSystemHandler.highlightTiles = !buildingSystemHandler.highlightTiles;
        buildingMenu.SetActive(buildingSystemHandler.highlightTiles);
    }
}
