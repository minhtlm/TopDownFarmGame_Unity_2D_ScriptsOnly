using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    private int selectedHotbarSlot = 0;
    private float speed = 5.0f;
    private bool isUsingTool = false;
    private Rigidbody2D rigidbody2d;
    private Vector2 moveInput;
    private Vector2 lookDirection;
    private Animator animator;
    private TreeController tree;
    private IDestructible currentInteractable;
    private ItemStack currentItem = null;
    private PlayerInventory playerInventory;
    [SerializeField] private ShopHouseController shopHouseController;
    [SerializeField] private UIInventoryManager inventoryManager;
    [SerializeField] private UIHotBarManager hotBarManager;

    // Input actions
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction ToggleInventoryAction; // Key: tab
    [SerializeField] private InputAction UseToolAction; // Key: F
    [SerializeField] private InputAction InteractAction; // Key: E
    [SerializeField] private InputAction hotKeyAction; // Key: 1-9, 0, -, =

    public Animator hairAnimator;
    public Animator toolAnimator;


    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        ToggleInventoryAction.Enable();
        UseToolAction.Enable();
        InteractAction.Enable();
        hotKeyAction.Enable();

        ToggleInventoryAction.performed += OnToggleInventoryPerformed;
        UseToolAction.performed += (UseTool);
        InteractAction.performed += (OnInteraction);
        hotKeyAction.performed += OnHotkeyPerformed;
    }

    void OnDisable()
    {
        ToggleInventoryAction.performed -= OnToggleInventoryPerformed;
        UseToolAction.performed -= (UseTool);
        InteractAction.performed -= (OnInteraction);
        hotKeyAction.performed -= OnHotkeyPerformed;

        ToggleInventoryAction.Disable();
        UseToolAction.Disable();
        InteractAction.Disable();
        hotKeyAction.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        moveAction.Enable();
        if (inventoryManager == null)
        {
            inventoryManager = FindObjectOfType<UIInventoryManager>();
        }
        if (hotBarManager == null)
        {
            hotBarManager = FindObjectOfType<UIHotBarManager>();
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
        if (currentInteractable != null && currentInteractable.CanInteract(currentItem.itemDefinition.itemName))
        {
            currentInteractable.OnInteractAnimation();
        }
        else
        {
            Debug.Log("No interactable object found: ");
        }
    }

    void OnInteraction(InputAction.CallbackContext context)
    {
        shopHouseController = ShopHouseController.Instance;
        if (shopHouseController != null)
        {
            Debug.Log("Interacting with shop house");
            shopHouseController.EnterShopHouse(gameObject);
        }
    }   

    void UseTool(InputAction.CallbackContext context)
    {
        // Check if the selected hotbar slot has the correct tool type
        if (playerInventory == null)
        {
            playerInventory = PlayerInventory.Instance;
        }
        if (playerInventory == null)
        {
            Debug.LogError("PlayerInventory not found");
            return;
        }
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
                currentItem = item;
            }
        }

        // Stop movement during tool use
        moveInput = Vector2.zero;
        Set_Speed(new Animator[] { animator, hairAnimator, toolAnimator });

        // Set the tool animation
        string animTrigger = currentItem.itemDefinition.itemName + "Trigger";
        Set_Trigger(new Animator[] { animator, hairAnimator}, "axeTrigger");
        Set_Trigger(new Animator[] { toolAnimator }, animTrigger);
        isUsingTool = true;

        // Check if the tool can interact with the target
        string targetLayer = currentItem.itemDefinition.targetLayer;
        Collider2D hit = Physics2D.OverlapBox(rigidbody2d.position + new Vector2(lookDirection.x * 1.0f, 0.2f),
            new Vector2(1.0f, 0.8f), 0, LayerMask.GetMask(targetLayer));
        if (hit != null)
        {
            IDestructible interactable = hit.GetComponent<IDestructible>();
            if (interactable != null )
            {
                currentInteractable = interactable;
                interactable.Interact(currentItem.itemDefinition.itemName);
            }
        } else
        {
            currentInteractable = null;
        }
    }

    void OnToggleInventoryPerformed(InputAction.CallbackContext context)
    {
        if (inventoryManager != null)
        {
            inventoryManager.ToggleInventory();
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

            if (playerInventory == null)
            {
                playerInventory = PlayerInventory.Instance;
                if (playerInventory == null)
                {
                    Debug.LogError("PlayerInventory not found");
                    return;
                }
            }
            ItemStack item = playerInventory.GetItem(slotSelectedIndex);
            if (item != null)
            {
                Debug.Log("Selected item: " + item.itemDefinition.itemName);
            }
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
}
