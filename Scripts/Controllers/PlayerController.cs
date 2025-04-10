using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private int selectedHotbarSlot = 0;
    private float speed = 5.0f;
    private bool isUsingTool = false;
    private Rigidbody2D rigidbody2d;
    private Vector2 moveInput;
    private Vector2 lookDirection;
    private Animator animator;
    private TreeController tree;
    private IDestructible currentDestructible;
    private ItemDefinition currentItem = null;
    private PlayerInventory playerInventory;
    [SerializeField] private UIHandler_inventory inventoryManager;
    [SerializeField] private UIHandler_hotbar hotBarManager;

    // Input actions
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction showingInventoryAction; // Key: tab
    [SerializeField] private InputAction useToolAction; // Key: F
    [SerializeField] private InputAction interactAction; // Key: E
    [SerializeField] private InputAction hotKeyAction; // Key: 1-9, 0, -, =
    [SerializeField] private InputAction escapeAction; // Key: esc

    public Animator hairAnimator;
    public Animator toolAnimator;


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
    }

    void OnEnable()
    {
        moveAction.Enable();
        showingInventoryAction.Enable();
        useToolAction.Enable();
        interactAction.Enable();
        hotKeyAction.Enable();
        escapeAction.Enable();

        showingInventoryAction.performed += OnShowingInventoryPerformed;
        useToolAction.performed += (UseItem);
        interactAction.performed += (OnInteraction);
        hotKeyAction.performed += OnHotkeyPerformed;
        escapeAction.performed += (OnEscapePressed);
    }

    void OnDisable()
    {
        showingInventoryAction.performed -= OnShowingInventoryPerformed;
        useToolAction.performed -= (UseItem);
        interactAction.performed -= (OnInteraction);
        hotKeyAction.performed -= OnHotkeyPerformed;
        escapeAction.performed -= (OnEscapePressed);

        moveAction.Disable();
        showingInventoryAction.Disable();
        useToolAction.Disable();
        interactAction.Disable();
        hotKeyAction.Disable();
        escapeAction.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (inventoryManager == null)
        {
            inventoryManager = FindObjectOfType<UIHandler_inventory>();
        }
        if (hotBarManager == null)
        {
            hotBarManager = FindObjectOfType<UIHandler_hotbar>();
        }

        playerInventory = PlayerInventory.Instance;
        if (playerInventory == null)
        {
            Debug.LogError("PlayerInventory not found");
            return;
        }
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
        Vector2 position = (Vector2)rigidbody2d.position + moveInput * speed * Time.deltaTime;
        rigidbody2d.MovePosition(position);
    }

    // Animation event
    void OnToolUseComplete()
    {
        isUsingTool = false;
        speed = 5.0f;
    }

    // Shake the tree by Animation event
    void OnInteractAnimation()
    {
        if (currentDestructible != null && currentDestructible.CanInteract(currentItem.ItemName))
        {
            currentDestructible.OnInteractAnimation();
        }
        else
        {
            Debug.Log("No destructible object found: ");
        }
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
        if (selectedHotbarSlot >= 0 && selectedHotbarSlot < playerInventory.GetItems().Count)
        {
            ItemStack item = playerInventory.GetItem(selectedHotbarSlot);
            if (item == null)
            {
                Debug.Log("No item in the selected hotbar slot");
                return;
            }
            else 
            {
                currentItem = item.itemDefinition;
            }

            if (currentItem is ToolDefinition tool)
            {
                UseTool(tool);
            }
            else if (currentItem is ConsumableDefinition consumable)
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
    }

    void UseTool(ToolDefinition tool)
    {
        // Stop movement during tool use
        moveInput = Vector2.zero;
        Set_Speed(new Animator[] { animator, hairAnimator, toolAnimator });

        // Set the tool animation
        Set_Trigger(new Animator[] { animator, hairAnimator}, "axeTrigger");
        Set_Trigger(new Animator[] { toolAnimator }, tool.AnimatorTrigger);
        isUsingTool = true;

        // Check if the tool can interact with the target
        Collider2D hit = Physics2D.OverlapBox(rigidbody2d.position + new Vector2(lookDirection.x * 1.0f, 0.2f),
            new Vector2(1.0f, 0.8f), 0, LayerMask.GetMask(tool.TargetLayer));
        if (hit != null)
        {
            IDestructible destructible = hit.GetComponent<IDestructible>();
            if (destructible != null )
            {
                currentDestructible = destructible;
                destructible.Interact(tool.ItemName);
            }
        } else
        {
            currentDestructible = null;
        }
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

    void OnHotkeyPerformed(InputAction.CallbackContext context)
    {
        string keyPressed = context.control.name;
        int slotSelectedIndex = -1;

        switch (keyPressed)
        {
            case "1": slotSelectedIndex = 0; break;
            case "2": slotSelectedIndex = 1; break;
            case "3": slotSelectedIndex = 2; break;
            case "4": slotSelectedIndex = 3; break;
            case "5": slotSelectedIndex = 4; break;
            case "6": slotSelectedIndex = 5; break;
            case "7": slotSelectedIndex = 6; break;
            case "8": slotSelectedIndex = 7; break;
            case "9": slotSelectedIndex = 8; break;
            case "0": slotSelectedIndex = 9; break;
            case "minus": slotSelectedIndex = 10; break;
            case "equals": slotSelectedIndex = 11; break;
            default: break;
        }

        if (slotSelectedIndex >= 0 && slotSelectedIndex < hotBarManager.hotBarSlots.Count)
        {
            selectedHotbarSlot = slotSelectedIndex;

            // Set selected slot in the UI
            if (hotBarManager != null)
            {
                hotBarManager.SetSelectedSlot(slotSelectedIndex);
            }

            ItemStack item = playerInventory.GetItem(slotSelectedIndex);
            if (!playerInventory.IsEmptySlot(slotSelectedIndex))
            {
                Debug.Log("Selected item: " + item.itemDefinition.ItemName);
            }
        }
    }

    void OnEscapePressed(InputAction.CallbackContext context)
    {
        if (IClosableUI.openingUI != null)
        {
            IClosableUI.openingUI.CloseUI();
        }
        else
        {
            Debug.Log("Open Menu");
        }
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

    public void DisableGameplayActions()
    {
        moveAction.Disable();
        showingInventoryAction.Disable();
        useToolAction.Disable();
        interactAction.Disable();
        hotKeyAction.Disable();
    }

    public void EnableGameplayActions()
    {
        moveAction.Enable();
        showingInventoryAction.Enable();
        useToolAction.Enable();
        interactAction.Enable();
        hotKeyAction.Enable();
    }
}