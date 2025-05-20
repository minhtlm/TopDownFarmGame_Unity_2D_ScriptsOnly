using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIHandler_StartMenu : MonoBehaviour
{
    private UIDocument uiDocument;
    private Button newGameButton;
    private Button continueButton;
    private Button exitButton;
    private string savePath;

    // Start is called before the first frame update
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        newGameButton = uiDocument.rootVisualElement.Q<Button>("NewGameButton");
        continueButton = uiDocument.rootVisualElement.Q<Button>("ContinueButton");
        exitButton = uiDocument.rootVisualElement.Q<Button>("ExitButton");
        savePath = Application.persistentDataPath + "/save.json";

        if (File.Exists(savePath))
        {
            continueButton.SetEnabled(true);
        }
        else
        {
            continueButton.SetEnabled(false);
        }

        // Set up button click events
        newGameButton.clicked += OnNewGameButtonClicked;
        continueButton.clicked += OnContinueButtonClicked;
        exitButton.clicked += OnExitButtonClicked;        
    }

    private void OnNewGameButtonClicked()
    {
        UIHandler_WaitingScreen.Instance.ShowWaitingScreen();

        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("MainScene");
    }

    private void OnContinueButtonClicked()
    {
        UIHandler_WaitingScreen.Instance.ShowWaitingScreen();
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("MainScene");
    }

    private void OnExitButtonClicked()
    {
        Debug.Log("Exit button clicked");
        Application.Quit();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainScene")
        {
            // Load the game data here
            SaveSystem.Instance.LoadGame();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UIHandler_WaitingScreen.Instance.HideWaitingScreen();
        }
    }
}
