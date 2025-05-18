using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

public class UIHandler_screenFader : MonoBehaviour
{
    private float fadeDuration = 1.0f;
    private VisualElement screenFader;
    private VisualElement content;
    private Label endOfDayLabel;
    private CinemachineVirtualCamera virtualCamera;
    private float originalDamping;
    private CinemachineFramingTransposer transposer;

    public static UIHandler_screenFader Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        screenFader = uiDocument.rootVisualElement.Q<VisualElement>("ScreenFader");
        content = uiDocument.rootVisualElement.Q<VisualElement>("Content");
        endOfDayLabel = uiDocument.rootVisualElement.Q<Label>("EndOfDayLabel");
        content.style.display = DisplayStyle.None;
        screenFader.style.display = DisplayStyle.None;

        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            originalDamping = transposer.m_XDamping;
        }
    }

    public void FadeIn()
    {
        screenFader.style.display = DisplayStyle.Flex;
        screenFader.AddToClassList("show");
        PlayerController.Instance.DisableGameplayActions();
    }

    public void FadeOut()
    {
        screenFader.RemoveFromClassList("show");
        PlayerController.Instance.EnableGameplayActions();
        screenFader.style.display = DisplayStyle.None;
        content.style.display = DisplayStyle.None;
    }

    public IEnumerator FadeTransition(System.Action preFadeAction, System.Action teleportAction, System.Action afterFadeAction = null)
    {
        // Set damping = 0 to make camera move instantly
        transposer.m_XDamping = 0;
        transposer.m_YDamping = 0;
        FadeIn();

        yield return new WaitForSeconds(fadeDuration/2);

        preFadeAction?.Invoke();

        yield return null;

        if (teleportAction != null)
        {
            teleportAction.Invoke();
        }

        yield return new WaitForSeconds(fadeDuration/2);

        FadeOut();
        // Reset damping
        transposer.m_XDamping = originalDamping;
        transposer.m_YDamping = originalDamping;

        // Executing the afterFadeAction
        afterFadeAction.Invoke();
    }

    public IEnumerator FadeEndOfDay(Action action = null)
    {
        endOfDayLabel.text = new string("End of Day " + TimeManager.Instance.Day);

        FadeIn();

        yield return new WaitForSeconds(fadeDuration/2);

        content.style.display = DisplayStyle.Flex;

        action?.Invoke();
        yield return null;

        PlayerStats.Instance.ResetStats();
        // GlobalLightController.Instance.UpdateLightByHour(TimeManager.Instance.Hour);

        yield return new WaitForSeconds(3.0f);

        FadeOut();
    }
}
