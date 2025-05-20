using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIHandler_PauseMenu : IClosableUI
{
    [SerializeField] private UIHandler_hotbar hotbarUIHandler;
    private UIDocument uiDocument;
    private Button exitButton;

    // Start is called before the first frame update
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        exitButton = uiDocument.rootVisualElement.Q<Button>("ExitButton");

        uiDocument.rootVisualElement.style.display = DisplayStyle.None;

        if (hotbarUIHandler == null)
        {
            hotbarUIHandler = FindObjectOfType<UIHandler_hotbar>();
        }

        exitButton.clicked += OnExitButtonClicked;
    }

    private void OnExitButtonClicked()
    {
        Debug.Log("Exit button clicked");
        Application.Quit();
    }

    public override void ShowUI()
    {
        uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        IClosableUI.openingUI = this;
        PlayerController.Instance.DisableGameplayActions();

        TimeManager.Instance.StopTime(); // Pause the time
        PlayerStats.Instance.StopDecreaseStats(); // Pause the stats decrease

        if (hotbarUIHandler != null)
        {
            hotbarUIHandler.Hide();
        }
    }

    public override void CloseUI()
    {
        uiDocument.rootVisualElement.style.display = DisplayStyle.None;
        IClosableUI.openingUI = null;
        PlayerController.Instance.EnableGameplayActions();
        TimeManager.Instance.StartTime(); // Resume the time
        PlayerStats.Instance.StartDecreaseStats(); // Resume the stats decrease

        if (hotbarUIHandler != null)
        {
            hotbarUIHandler.Show();
        }
    }
}
