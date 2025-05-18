using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIHandler_SleepConfirm : MonoBehaviour
{
    private UIDocument uiDocument;

    [SerializeField] private UIHandler_hotbar hotbar;

    // Start is called before the first frame update
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        VisualElement root = uiDocument.rootVisualElement;
        root.style.display = DisplayStyle.None;

        Button yesButton = root.Q<Button>("YesButton");
        yesButton.RegisterCallback<ClickEvent>(ev => {
            StartCoroutine(UIHandler_screenFader.Instance.FadeEndOfDay(
                () => {
                    TimeManager.Instance.SetNextDay();
                }
            ));
            HideSleepConfirm();
        });

        Button noButton = root.Q<Button>("NoButton");
        noButton.RegisterCallback<ClickEvent>(ev => {
            HideSleepConfirm();
        });
    }

    public void ShowSleepConfirm()
    {
        uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;

        PlayerController.Instance.DisableGameplayActions();
        hotbar.Hide();
    }

    public void HideSleepConfirm()
    {
        uiDocument.rootVisualElement.style.display = DisplayStyle.None;

        PlayerController.Instance.EnableGameplayActions();
        hotbar.Show();
    }
}
