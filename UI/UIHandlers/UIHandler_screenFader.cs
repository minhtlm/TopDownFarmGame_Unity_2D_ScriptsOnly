using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

public class UIHandler_screenFader : MonoBehaviour
{
    private float fadeDuration = 1.0f;
    private VisualElement screenFader;
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

        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            originalDamping = transposer.m_XDamping;
        }
    }

    void FadeIn()
    {
        screenFader.AddToClassList("show");
        PlayerController.Instance.DisableGameplayActions();
    }

    void FadeOut()
    {
        screenFader.RemoveFromClassList("show");
        PlayerController.Instance.EnableGameplayActions();
    }

    public IEnumerator FadeTransition(System.Action teleportAction)
    {
        // Set damping = 0 to make camera move instantly
        transposer.m_XDamping = 0;
        transposer.m_YDamping = 0;

        FadeIn();
        yield return new WaitForSeconds(fadeDuration/2);
        if (teleportAction != null)
        {
            teleportAction.Invoke();
        }
        yield return new WaitForSeconds(fadeDuration/2);
        FadeOut();

        // Reset damping
        transposer.m_XDamping = originalDamping;
        transposer.m_YDamping = originalDamping;
    }
}
