using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class UIHandler_Popup : MonoBehaviour
{
    private UIDocument uiDocument;
    private Label popupLabel;
    private Coroutine fishWaitingCoroutine;
    private Coroutine fishBiteCoroutine;
    private float offset = -170.0f;

    private float fishBitePopupDuration = 1.0f;
    public float FishBitePopupDuration => fishBitePopupDuration;

    [SerializeField] Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        VisualElement root = uiDocument.rootVisualElement;
        popupLabel = root.Q<Label>("PopupLabel");

        HidePopup();
        InitilizePopup();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void InitilizePopup()
    {
        Vector3 playerPosition = PlayerController.Instance.transform.position;
        Vector2 screenPosition = mainCamera.WorldToScreenPoint(playerPosition);

        popupLabel.style.left = screenPosition.x;
        popupLabel.style.top = screenPosition.y + offset;
    }

    IEnumerator FishWaitingAnimation()
    {
        int dotCount = 0;

        while (true)
        {
            dotCount = (dotCount + 1) % 4;
            string dots = new string('.', dotCount);
            popupLabel.text = "Fishing" + dots;
            yield return new WaitForSeconds(0.5f);
        }
    }

    void HidePopup()
    {
        popupLabel.style.display = DisplayStyle.None;
    }

    void ShowPopup()
    {
        popupLabel.style.display = DisplayStyle.Flex;
    }

    public void StartFishWaitingPopup()
    {
        ShowPopup();
        fishWaitingCoroutine = StartCoroutine(FishWaitingAnimation());
    }

    public void StopFishWaitingPopup()
    {
        if (fishWaitingCoroutine != null)
        {
            StopCoroutine(fishWaitingCoroutine);
            fishWaitingCoroutine = null;
        }

        HidePopup();
    }

    public void StartFishBitePopup()
    {
        popupLabel.text = "Fish on!";
        ShowPopup();
        Vector2 currentScale = Vector2.one;
        popupLabel.style.scale = currentScale;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(
            DOTween.To(
                () => currentScale,
                x => {
                    currentScale = x;
                    popupLabel.style.scale = currentScale;
                },
                Vector2.one * 3.0f,
                0.3f
            ).SetEase(Ease.OutQuad)
        );

        sequence.AppendInterval(0.2f);

        sequence.Append(
            DOTween.To(
                () => currentScale,
                x => {
                    currentScale = x;
                    popupLabel.style.scale = currentScale;
                },
                Vector2.zero,
                fishBitePopupDuration - 0.3f
            ).SetEase(Ease.OutQuad)
        );
        
        sequence.OnComplete(() => {
            HidePopup();
            popupLabel.style.scale = Vector2.one; // Reset scale for next use
        });
    }
}
