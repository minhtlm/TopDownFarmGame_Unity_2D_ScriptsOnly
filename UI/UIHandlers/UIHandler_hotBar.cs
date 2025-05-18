using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UIHandler_hotbar : MonoBehaviour
{
    private UIDocument uiDocument;
    private PlayerInventory playerInventory;
    private bool isVisible = true;
    private int selectedHotbarSlot = 0;


    public List<VisualElement> hotBarSlots = new List<VisualElement>();

    [SerializeField] private InputAction hotKeyAction; // Key: 1-9, 0, -, =


    // Start is called before the first frame update
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        VisualElement root = uiDocument.rootVisualElement;
        VisualElement hotBarContainer = root.Q<VisualElement>("HotBarContainer");
        if (hotBarContainer != null)
        {
            foreach (VisualElement slot in hotBarContainer.Children())
            {
                hotBarSlots.Add(slot);
            }
        }
        else
        {
            Debug.LogError("HotBarContainer not found");
            return;
        }

        playerInventory = PlayerInventory.Instance;
        if (playerInventory == null)
        {
            Debug.LogError("PlayerInventory not found");
            return;
        }

        SetSelectedSlot(selectedHotbarSlot);

        playerInventory.OnInventoryChanged += UpdateHotbarUI;

        UpdateHotbarUI();
    }

    void OnEnable()
    {
        EnableInputActions();
    }

    void OnDisable()
    {
        DisableInputActions();
    }

    void OnDestroy()
    {
        playerInventory.OnInventoryChanged -= UpdateHotbarUI;
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

        if (slotSelectedIndex >= 0 && slotSelectedIndex < hotBarSlots.Count)
        {
            selectedHotbarSlot = slotSelectedIndex;

            // Set selected slot in the UI
            SetSelectedSlot(slotSelectedIndex);
        }
    }

    void UpdateHotbarUI()
    {
        if (!isVisible) return;

        ClearAllSlots();

        List<ItemStack> playerItems = playerInventory.GetItems();

        for (int i = 0; i < hotBarSlots.Count && i < playerItems.Count; i++)
        {
            if (playerInventory.IsEmptySlot(i)) continue;

            ItemStack currentItem = playerItems[i];
            VisualElement slot = hotBarSlots[i];
            VisualElement icon = slot.Q<VisualElement>("Item");
            if (icon != null)
            {
                icon.style.backgroundImage = currentItem.itemDefinition.ItemSprite.texture;
            }

            Label label = slot.Q<Label>("Quantity");
            if (label != null && currentItem.quantity > 1)
            {
                label.text = currentItem.quantity.ToString();
            }
        }
    }

    void ClearAllSlots()
    {
        foreach (VisualElement slot in hotBarSlots)
        {
            VisualElement icon = slot.Q<VisualElement>("Item");
            if (icon != null)
            {
                icon.style.backgroundImage = null;
            }

            Label label = slot.Q<Label>("Quantity");
            if (label != null)
            {
                label.text = "";
            }
        }
    }

    void EnableInputActions()
    {
        if (hotKeyAction != null)
        {
            hotKeyAction.Enable();
            hotKeyAction.performed += OnHotkeyPerformed;
        }
        else
        {
            Debug.LogError("Hotkey action not assigned");
        }
    }

    void DisableInputActions()
    {
        if (hotKeyAction != null)
        {
            hotKeyAction.Disable();
            hotKeyAction.performed -= OnHotkeyPerformed;
        }
        else
        {
            Debug.LogError("Hotkey action not assigned");
        }
    }

    public void Hide()
    {
        isVisible = false;
        DisableInputActions();

        if (uiDocument != null)
        {
            uiDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
        else
        {
            Debug.LogError("UIDocument not found to hide the hotbar");
        }
    }

    public void Show()
    {
        isVisible = true;
        EnableInputActions();
        
        if (uiDocument != null)
        {
            uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            UpdateHotbarUI();
        }
        else
        {
            Debug.LogError("UIDocument not found to show the hotbar");
        }
    }

    public void SetSelectedSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= hotBarSlots.Count) return;

        foreach (VisualElement slot in hotBarSlots)
        {
            slot.Q<VisualElement>("Item").RemoveFromClassList("selected-slot");
        }

        // Highlight selected slot
        hotBarSlots[slotIndex].Q<VisualElement>("Item").AddToClassList("selected-slot");
    }

    public ItemStack GetSelectedItem()
    {
        if (selectedHotbarSlot < 0 || selectedHotbarSlot >= hotBarSlots.Count) return null;

        return playerInventory.GetItem(selectedHotbarSlot);
    }
}
