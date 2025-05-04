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
    private float fishingBarMinY = 0.0f;
    private float fishIconMaxY;
    private float fishIconY;
    private float fishHeight;
    private float playerBarHeight;
    private float playerBarY;
    private float playerBarMaxY;
    private float velocity = 0.0f;
    private float catchProgress = 0.0f;
    private const float liftImpulse = 50.0f;
    private const float maxVelocity = 150.0f;
    private const float liftForce = 400.0f;
    private const float gravity = -100.0f;
    private bool isLifting = false;
    private Tween fishTween;
    [SerializeField] private float progressChangeSpeed = 0.1f;
    [SerializeField] private float fishSpeed = 100.0f;
    [SerializeField] private float easyFishMovingPercent = 0.7f;
    [SerializeField] private float fishMovingMinDistance = 100.0f;
    [SerializeField] private InputAction catchFishAction;

    void OnEnable()
    {
        catchFishAction.Enable();
        catchFishAction.performed += OnCatchPerformed;
        catchFishAction.canceled += OnCatchCanceled;
    }

    void OnDisable()
    {
        catchFishAction.Disable();
        catchFishAction.performed -= OnCatchPerformed;
        catchFishAction.canceled -= OnCatchCanceled;
    }

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        fishIcon = root.Q<VisualElement>("FishIcon");
        fishingBar = root.Q<VisualElement>("FishingBar");
        playerBar = root.Q<VisualElement>("PlayerBar");
        progressBar = root.Q<VisualElement>("ProgressBar");

        WaitUntilLayoutReady();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayerBar();
        UpdateProgressBar();
    }

    void OnDestroy()
    {
        if (fishTween != null)
        {
            fishTween.Kill();
        }
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
    private void WaitUntilLayoutReady()
    {
        float fishingBarHeight = fishingBar.resolvedStyle.height;
        fishHeight = fishIcon.resolvedStyle.height;
        playerBarHeight = playerBar.resolvedStyle.height;

        // Wait until the layout is ready
        if (float.IsNaN(fishingBarHeight) || float.IsNaN(fishHeight) || float.IsNaN(playerBarHeight))
        {
            root.schedule.Execute(WaitUntilLayoutReady).ExecuteLater(1);
            return;
        }

        fishIconMaxY = fishingBarHeight - fishHeight;
        playerBarMaxY = fishingBarHeight - playerBarHeight;
        
        MoveFishToNextPosition();
    }

    private void MoveFishToNextPosition()
    {
        fishTween?.Kill(); // Kill the previous fish tween if it exists

        float currentY = fishIcon.resolvedStyle.bottom;

        // Generate a random target Y position within the fishing bar range, ensuring it's not too close to the current position
        float targetY;
        do {
            targetY = Random.Range(fishingBarMinY, fishIconMaxY);
        } while (Mathf.Abs(targetY - currentY) < fishMovingMinDistance);
        
        float distance = Mathf.Abs(targetY - currentY);
        float duration = distance / fishSpeed;
        Ease easing = Random.value < easyFishMovingPercent ? Ease.Linear : Ease.OutQuad;

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

        if (catchProgress >= 1.0f)
        {
            // Fish caught successfully
            Debug.Log("Fish Caught!");
        }
        else if (catchProgress <= 0.0f)
        {
            // Fish escaped
            Debug.Log("Fish Escaped!");
        }
    }
}
