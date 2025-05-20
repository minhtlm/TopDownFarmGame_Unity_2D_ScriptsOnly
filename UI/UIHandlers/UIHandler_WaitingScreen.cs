using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIHandler_WaitingScreen : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement waitingScreen;

    public static UIHandler_WaitingScreen Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        waitingScreen = uiDocument.rootVisualElement.Q<VisualElement>("Background");
        HideWaitingScreen();
    }

    public void ShowWaitingScreen()
    {
        waitingScreen.style.display = DisplayStyle.Flex;
    }

    public void HideWaitingScreen()
    {
        waitingScreen.style.display = DisplayStyle.None;
    }
}
