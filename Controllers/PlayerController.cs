using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private float speed = 5.0f;
    private bool isUsingTool = false;
    private bool isFishing = false;
    public bool IsFishing => isFishing;
    private Vector2 moveInput;
    private Vector2 lookDirection;
    private IDestructible currentDestructible;
    private ItemDefinition currentItem = null;
    private PlayerInventory playerInventory;
    private AudioSource audioSource;
    private Coroutine fishingCoroutine;

    private Rigidbody2D rigidbody2d;
    public Rigidbody2D Rigidbody2D => rigidbody2d;

    [SerializeField] private UIHandler_inventory inventoryManager;
    [SerializeField] private UIHandler_hotbar hotBarManager;
    [SerializeField] private UIHandler_FishingMinigame fishingMinigame;
    [SerializeField] private UIHandler_Popup popupUI;
    [SerializeField] private Animator hairAnimator;
    [SerializeField] private Animator toolAnimator;
    [SerializeField] private Animator animator;

    // Input actions
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction showingInventoryAction; // Key: tab
    [SerializeField] private InputAction useToolAction; // Key: F
    [SerializeField] private InputAction interactAction; // Key: E
    [SerializeField] private InputAction escapeAction; // Key: esc

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
        
        DontDestroyOnLoad(gameObject);

        rigidbody2d = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        moveAction.Enable();
        showingInventoryAction.Enable();
        useToolAction.Enable();
        interactAction.Enable();
        escapeAction.Enable();

        showingInventoryAction.performed += OnShowingInventoryPerformed;
        useToolAction.performed += (UseItem);
        interactAction.performed += (OnInteraction);
        escapeAction.performed += (OnEscapePressed);
    }

    void OnDisable()
    {
        showingInventoryAction.performed -= OnShowingInventoryPerformed;
        useToolAction.performed -= (UseItem);
        interactAction.performed -= (OnInteraction);
        escapeAction.performed -= (OnEscapePressed);

        moveAction.Disable();
        showingInventoryAction.Disable();
        useToolAction.Disable();
        interactAction.Disable();
        escapeAction.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (inventoryManager == null)
        {
            inventoryManager = FindObjectOfType<UIHandler_inventory>();
        }
        if (hotBarManager == null)
        {
            hotBarManager = FindObjectOfType<UIHandler_hotbar>();
        }
        if (fishingMinigame == null)
        {
            fishingMinigame = FindObjectOfType<UIHandler_FishingMinigame>();
        }
        if (popupUI == null)
        {
            popupUI = FindObjectOfType<UIHandler_Popup>();
        }
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        playerInventory = PlayerInventory.Instance;
        if (playerInventory == null)
        {
            Debug.LogError("PlayerInventory not found");
            return;
        }

        fishingMinigame.FishingFinished += OnFishingFinished; // Subscribe to the fishing finished event
    }

    // Update is called once per frame
    void Update()
    {
        if (!isUsingTool)
        {
            moveInput = moveAction.ReadValue<Vector2>();
            if (!Mathf.Approximately(moveInput.x, 0.0f))
            {
                lookDirection.Set(moveInput.x, 0f);
                lookDirection.Normalize();
            }
            Set_Look(new Animator[] { animator, hairAnimator, toolAnimator });
            Set_Speed(new Animator[] { animator, hairAnimator, toolAnimator });
        }
    }

    void FixedUpdate()
    {
        Move();
        if (moveInput != Vector2.zero)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        } 
        else
        {
            audioSource.Stop();
        }
    }

    void Move()
    {
        Vector2 position = (Vector2)rigidbody2d.position + moveInput * speed * Time.deltaTime;
        rigidbody2d.MovePosition(position);
    }

    // When the player interacts with an interactable object (key: E)
    void OnInteraction(InputAction.CallbackContext context)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position + new Vector3(0, 0.5f, 0), 1.0f, LayerMask.GetMask("Interactable"));
        if (hits != null && hits.Length > 0)
        {
            foreach (Collider2D hit in hits)
            {
                if (!hit.isTrigger)
                {
                    IInteractable interactable = hit.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        interactable.OnInteract();
                    }
                }
            }
        }
    }   

    // When the player uses a tool (key: F)
    void UseItem(InputAction.CallbackContext context)
    { 
        ItemStack item = hotBarManager.GetSelectedItem();
        if (item == null)
        {
            Debug.Log("No item in the selected hotbar slot");
            return;
        }
        else 
        {
            currentItem = item.itemDefinition;
        }

        if (currentItem is Tool tool)
        {
            UseTool(tool);
        }
        else if (currentItem is Consumable consumable)
        {
            if (consumable.UseItem())
            {
                playerInventory.RemoveItem(currentItem, 1);
            }
            else
            {
                Debug.Log("Cannot use consumable: " + consumable.ItemName);
            }
        }
    }

    void UseTool(Tool tool)
    {
        // Stop movement during tool use
        moveInput = Vector2.zero;
        Set_Speed(new Animator[] { animator, hairAnimator, toolAnimator });

        // Set the tool animation
        Set_Trigger(new Animator[] { animator, hairAnimator, toolAnimator }, tool.AnimatorTrigger);

        // Disable all the gameplay actions
        isUsingTool = true;
        DisableGameplayActions();

        // Check if the tool can interact with the target
        Collider2D hit = Physics2D.OverlapBox(rigidbody2d.position + new Vector2(lookDirection.x * 1.0f, 0.2f),
            new Vector2(1.0f, 0.8f), 0, LayerMask.GetMask(tool.TargetLayer));

        if (hit != null)
        {
            if (tool.ItemName == "fishingrod")
            {
                Collider2D overLap = Physics2D.OverlapBox(rigidbody2d.position + new Vector2(lookDirection.x * 1.5f, -1f),
                    new Vector2(1.0f, 0.8f), 0, LayerMask.GetMask("Bridge"));

                if (overLap == null)
                {
                    isFishing = true;
                    fishingCoroutine = StartCoroutine(FishingCoroutine());
                }
                else
                {
                    Debug.Log("Cannot fish here, no water");
                }
            }
            else
            {
                IDestructible destructible = hit.GetComponent<IDestructible>();
                if (destructible != null )
                {
                    currentDestructible = destructible;
                    destructible.Interact(tool.ItemName);
                }
            }
        } else
        {
            currentDestructible = null;
        }
    }

    IEnumerator FishingCoroutine()
    {
        // Waiting for the fish to bite
        float waitTime = Random.Range(3.0f, 5.0f);
        yield return new WaitForSeconds(waitTime);

        popupUI.HidePopup(); // Stop and hide the fishing popup

        popupUI.StartFishBitePopup(); // Show the fish bite popup
        yield return new WaitForSeconds(popupUI.FishBitePopupDuration); // Wait for the popup to finish

        Set_Int(new Animator[] { animator, hairAnimator, toolAnimator }, "FishingState", 2); // Start the reeling animation

        fishingMinigame.ShowFishingUI(); // Show the fishing UI
        isFishing = false; // Set the fishing state to false so that the player can't cancel the fishing minigame
    }

    void OnShowingInventoryPerformed(InputAction.CallbackContext context)
    {
        if (inventoryManager != null)
        {
            inventoryManager.ShowUI();
        } else
        {
            Debug.Log("InventoryManager is null");
        }
    }

    void OnEscapePressed(InputAction.CallbackContext context)
    {
        if (IClosableUI.openingUI != null)
        {
            IClosableUI.openingUI.CloseUI();
        }
        else if (isFishing)
        {
            StopFishing();
        }
        else
        {
            Debug.Log("Open Menu");
        }
    }

    void OnFishingFinished()
    {
        isFishing = false;
        Set_Int(new Animator[] { animator, hairAnimator, toolAnimator }, "FishingState", 3);
        OnToolUseComplete(); // Call the tool use complete method to re-enable gameplay actions
    }

    void Set_Look(Animator[] animators)
    {
        foreach (Animator animator in animators)
        {
            animator.SetFloat("Look X", lookDirection.x);
        }
    }

    void Set_Speed(Animator[] animators)
    {
        foreach (Animator animator in animators)
        {
            animator.SetFloat("Speed", moveInput.magnitude);
        }
    }

    void Set_Trigger(Animator[] animators, string trigger)
    {
        foreach (Animator animator in animators)
        {
            animator.SetTrigger(trigger);
        }
    }

    void Set_Int(Animator[] animators, string intName, int value)
    {
        foreach (Animator animator in animators)
        {
            animator.SetInteger(intName, value);
        }
    }

    public void DisableGameplayActions()
    {
        moveAction.Disable();
        showingInventoryAction.Disable();
        useToolAction.Disable();
        interactAction.Disable();
    }

    public void EnableGameplayActions()
    {
        moveAction.Enable();
        showingInventoryAction.Enable();
        useToolAction.Enable();
        interactAction.Enable();
    }

    public void StopFishing()
    {
        isFishing = false;
        if (fishingCoroutine != null)
        {
            StopCoroutine(fishingCoroutine);
            fishingCoroutine = null;
        }
        
        popupUI.HidePopup(); // Stop and hide the fishing popup

        Set_Int(new Animator[] { animator, hairAnimator, toolAnimator }, "FishingState", 0);
        OnToolUseComplete();
    }

    
    // ------Animation event below-------- //

    public void OnToolUseComplete()
    {
        isUsingTool = false;
        EnableGameplayActions();
    }

    public void OnInteractAnimation()
    {
        if (currentDestructible != null && currentDestructible.CanInteract(currentItem.ItemName))
        {
            currentDestructible.OnInteractAnimation();
        }
    }

    public void OnFinishCast()
    {
        if (isFishing)
        {
            Set_Int(new Animator[] { animator, hairAnimator, toolAnimator }, "FishingState", 1);
            popupUI.StartFishWaitingPopup(); // Show the fishing popup
        }
        else
        {
            OnToolUseComplete();
            Set_Int(new Animator[] { animator, hairAnimator, toolAnimator }, "FishingState", 0);
        }
    }

    // -------End of animation event-------- //

    public PlayerData ToSerializableData()
    {
        return new PlayerData
        {
            _position = rigidbody2d.position,
            _lookDirection = lookDirection
        };
    }

    public void LoadFromSerializableData(PlayerData playerData)
    {
        rigidbody2d.position = playerData._position;
        lookDirection = playerData._lookDirection;
        Set_Look(new Animator[] { animator, hairAnimator, toolAnimator });
    }

    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("AudioSource or AudioClip is null");
        }
    }
}