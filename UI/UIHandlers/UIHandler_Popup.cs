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
    private Sequence fishBiteSequence;
    [SerializeField] private float offset = -100.0f;

    private float fishBitePopupDuration = 1.0f;
    public float FishBitePopupDuration => fishBitePopupDuration;

    public static UIHandler_Popup Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        VisualElement root = uiDocument.rootVisualElement;
        popupLabel = root.Q<Label>("PopupLabel");

        // HidePopup();
    }

    void InitilizePopup()
    {
        Vector3 playerPosition = PlayerController.Instance.Rigidbody2D.position;

        // Lấy vị trí của camera trong thế giới
        Vector3 cameraPosition = Camera.main.transform.position;

        // Tính toán vị trí của Player tương đối với camera
        Vector3 relativePosition = playerPosition - cameraPosition;

        // Chuyển đổi vị trí tương đối sang tọa độ viewport
        Vector3 viewportPosition = Camera.main.WorldToViewportPoint(playerPosition);

        // Chuyển đổi tọa độ viewport sang tọa độ màn hình
        Vector2 screenPosition = new Vector2(
            viewportPosition.x * Screen.width,
            viewportPosition.y * Screen.height
        );

        // Chuyển đổi tọa độ màn hình sang tọa độ UI Toolkit
        float uiLeft = screenPosition.x - (popupLabel.resolvedStyle.width / 2);
        float uiTop = Screen.height - screenPosition.y - (popupLabel.resolvedStyle.height / 2);

        // Đặt vị trí cho popupLabel
        popupLabel.style.left = uiLeft;
        popupLabel.style.top = uiTop;
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

    void ShowPopup()
    {
        InitilizePopup();
        popupLabel.style.display = DisplayStyle.Flex;
    }

    public void CreatePopup(string text)
    {
        popupLabel.text = text;
        ShowPopup();
        popupLabel.style.scale = Vector2.one;

        float initTop = popupLabel.resolvedStyle.top;
        float targetTop = initTop - 50.0f;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(
            DOTween.To(
                () => popupLabel.resolvedStyle.top,
                x => popupLabel.style.top = x,
                targetTop,
                1.0f
            ).SetEase(Ease.OutQuad)
        );

        sequence.Join(
            DOTween.To(
                () => popupLabel.resolvedStyle.opacity,
                x => popupLabel.style.opacity = x,
                0.0f,
                1.0f
            ).SetEase(Ease.InQuad)
        );

        sequence.OnComplete(() => {
            HidePopup();
            popupLabel.style.top = initTop; // Reset position for next use
            popupLabel.style.opacity = 1.0f; // Reset opacity for next use
            Debug.Log("Popup animation completed.");
        });
    }

    public void HidePopup()
    {
        popupLabel.style.display = DisplayStyle.None;
        popupLabel.style.scale = Vector2.one; // Reset scale for next use
        popupLabel.style.opacity = 1.0f; // Reset opacity for next use
        popupLabel.text = ""; // Clear text for next use

        if (fishWaitingCoroutine != null)
        {
            StopCoroutine(fishWaitingCoroutine);
            fishWaitingCoroutine = null;
        }

        if (fishBiteSequence != null)
        {
            fishBiteSequence.Kill();
            fishBiteSequence = null;
        }
    }

    public void StartFishWaitingPopup()
    {
        ShowPopup();
        fishWaitingCoroutine = StartCoroutine(FishWaitingAnimation());
    }

    public void StartFishBitePopup()
    {
        popupLabel.text = "Fish on!";
        ShowPopup();
        Vector2 currentScale = Vector2.one;
        popupLabel.style.scale = currentScale;

        fishBiteSequence = DOTween.Sequence();

        fishBiteSequence.Append(
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

        fishBiteSequence.AppendInterval(0.2f);

        fishBiteSequence.Append(
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
        
        fishBiteSequence.OnComplete(() => {
            HidePopup(); // Reset the sequence and hide the popup
        });
    }
}
