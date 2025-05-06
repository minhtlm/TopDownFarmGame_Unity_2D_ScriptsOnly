using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UIHandler_FishingMinigame : MonoBehaviour
{
    private VisualElement root;
    private VisualElement fishIcon;
    private VisualElement fishingBar;
    private VisualElement playerBar;
    private VisualElement progressBar;
    private Label endingPopupLabel;
    private float fishingBarMinY = 0.0f;
    private float fishIconMaxY;
    private float fishIconY = 0.0f;
    private float fishHeight;
    private float playerBarHeight;
    private float playerBarY = 0.0f;
    private float playerBarMaxY;
    private float velocity = 0.0f;
    private float catchProgress = 0.0f;
    private const float liftImpulse = 50.0f;
    private const float maxVelocity = 150.0f;
    private const float liftForce = 400.0f;
    private const float gravity = -100.0f;
    private bool isLifting = false;
    private bool isActive = false;
    private Tween fishTween;
    private Sequence shakeUISeq;
    
    [SerializeField] private float progressChangeSpeed = 0.1f;
    [SerializeField] private float fishSpeed = 100.0f;
    [SerializeField] private float easyFishMovingPercent = 0.7f;
    [SerializeField] private float fishMovingMinDistance = 100.0f;
    [SerializeField] private InputAction catchFishAction;

    public event Action FishingFinished; // Event to notify when fishing is finished

    void OnEnable()
    {
        EnableFishingActions();
    }

    void OnDisable()
    {
        DisableUI();
    }

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        fishIcon = root.Q<VisualElement>("FishIcon");
        fishingBar = root.Q<VisualElement>("FishingBar");
        playerBar = root.Q<VisualElement>("PlayerBar");
        progressBar = root.Q<VisualElement>("ProgressBar");
        endingPopupLabel = root.Q<Label>("EndingPopup");

        HideFishingUI(); // Hide the fishing UI at the start
    }

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            UpdatePlayerBar();
            UpdateProgressBar();
        }
    }

    void OnDestroy()
    {
        ResetFishTween();
    }

    void OnCatchPerformed(InputAction.CallbackContext context)
    {
        isLifting = true;
        velocity = liftImpulse;
    }

    void OnCatchCanceled(InputAction.CallbackContext context)
    {
        isLifting = false;
        velocity = -liftImpulse;
    }

    // This method waits until the layout is ready
    private void AfterLayoutReady()
    {
        float fishingBarHeight = fishingBar.resolvedStyle.height;
        fishHeight = fishIcon.resolvedStyle.height;
        playerBarHeight = playerBar.resolvedStyle.height;

        fishIconMaxY = fishingBarHeight - fishHeight;
        playerBarMaxY = fishingBarHeight - playerBarHeight;
        
        MoveFishToNextPosition();
    }

    private void MoveFishToNextPosition()
    {
        if (!isActive) return; // Do not move fish if the fishing UI is not active

        ResetFishTween();

        float currentY = fishIcon.resolvedStyle.bottom;

        // Generate a random target Y position within the fishing bar range, ensuring it's not too close to the current position
        float targetY;
        do {
            targetY = UnityEngine.Random.Range(fishingBarMinY, fishIconMaxY);
        } while (Mathf.Abs(targetY - currentY) < fishMovingMinDistance);
        
        float distance = Mathf.Abs(targetY - currentY);
        float duration = distance / fishSpeed;
        Ease easing = UnityEngine.Random.value < easyFishMovingPercent ? Ease.Linear : Ease.OutQuad;

        // Move the fish and loop the movement
        fishTween = DOTween.To(
            () => fishIconY,
            y => {
                fishIconY = y;
                fishIcon.style.bottom = new StyleLength(y);
            },
            targetY,
            duration
        )
        .SetEase(easing)
        .OnComplete(() => MoveFishToNextPosition());
    }

    void ShakeUI()
    {
        Vector2 currentScale = Vector2.one;

        if (shakeUISeq != null)
        {
            shakeUISeq.Kill();
            shakeUISeq = null;
        }

        shakeUISeq = DOTween.Sequence();

        shakeUISeq.Append(
            DOTween.To(
                () => 0f,
                t => {
                    float offsetX = UnityEngine.Random.Range(-2f, 2f);
                    float offsetY = UnityEngine.Random.Range(-2f, 2f);
                    root.style.translate = new Translate(offsetX, offsetY); 
                },
                1f,
                1f
            )
            .SetEase(Ease.Linear)
        );

        shakeUISeq.Join(
            DOTween.To(
                () => currentScale,
                x => {
                    currentScale = x;
                    endingPopupLabel.style.scale = currentScale;
                },
                currentScale * 3.0f,
                1f
            )
            .SetEase(Ease.OutBack)
        );
        
        shakeUISeq.OnComplete(() => {

            // Reset the UI elements after shaking
            root.style.translate = new Translate(0, 0);
            endingPopupLabel.style.scale = Vector2.one;

            shakeUISeq.Kill(); // Kill the sequence after completion
            shakeUISeq = null;

            FishingFinished?.Invoke(); // Invoke the fishing finished event
            HideFishingUI();
        });
    }

    void UpdatePlayerBar()
    {
        // Input handling for player bar movement
        if (isLifting)
        {
            velocity += Time.deltaTime * liftForce;
        }
        
        velocity += Time.deltaTime * gravity; // Apply gravity

        velocity = Mathf.Clamp(velocity, -maxVelocity, maxVelocity); // Clamp the velocity to maxVelocity

        // Update player bar position based on velocity
        playerBarY += velocity * Time.deltaTime;
        playerBarY = Mathf.Clamp(playerBarY, fishingBarMinY, playerBarMaxY);

        playerBar.style.bottom = new StyleLength(playerBarY); // Apply to UI
    }

    void UpdateProgressBar()
    {
        bool isFishInsideBar = fishIconY + fishHeight >= playerBarY && fishIconY <= playerBarY + playerBarHeight;

        if (isFishInsideBar)
        {
            catchProgress += Time.deltaTime * progressChangeSpeed;
        }
        else
        {
            catchProgress -= Time.deltaTime * progressChangeSpeed;
        }
        catchProgress = Mathf.Clamp01(catchProgress);

        progressBar.style.height = Length.Percent(catchProgress * 100); // Update progress bar height

        if (catchProgress >= 1.0f) // Fish caught successfully
        {
            DisableUI(); // Disable the fishing UI

            endingPopupLabel.text = "Fish Caught!";
            ShakeUI();
        }
        else if (catchProgress <= 0.0f) // Fish escaped
        {
            DisableUI();

            endingPopupLabel.text = "Fish Escaped!";
            ShakeUI();
        }
    }

    void ResetFishTween()
    {
        if (fishTween != null)
        {
            fishTween.Kill();
            fishTween = null;
        }
    }

    void EnableFishingActions()
    {
        catchFishAction.Enable();
        catchFishAction.performed += OnCatchPerformed;
        catchFishAction.canceled += OnCatchCanceled;
    }

    void DisableUI()
    {
        isActive = false; // Disable the Update loop

        // Disable the Input actions
        catchFishAction.Disable();
        catchFishAction.performed -= OnCatchPerformed;
        catchFishAction.canceled -= OnCatchCanceled;

        ResetFishTween(); // Reset the fish tween
    }

    public void HideFishingUI()
    {
        DisableUI(); // Disable the UI and input actions
        root.style.display = DisplayStyle.None; // Hide the fishing UI

        // Reset all variables
        isLifting = false;
        velocity = 0.0f;
        catchProgress = 0.0f;
        playerBarY = 0.0f;
        fishIconY = 0.0f;

        // Reset the UI elements
        playerBar.style.bottom = new StyleLength(playerBarY);
        fishIcon.style.bottom = new StyleLength(fishIconY);
        progressBar.style.height = Length.Percent(catchProgress * 100);
        endingPopupLabel.text = string.Empty; // Clear the ending popup label

        PlayerController.Instance.EnableGameplayActions(); // Enable gameplay actions
    }

    public void ShowFishingUI()
    {
        isActive = true; // Enable the Update loop
        root.style.display = DisplayStyle.Flex; // Show the fishing UI

        PlayerController.Instance.DisableGameplayActions(); // Disable gameplay actions
        EnableFishingActions();

        root.RegisterCallback<GeometryChangedEvent>(evt => AfterLayoutReady()); // Wait for layout to be ready
    }
}
