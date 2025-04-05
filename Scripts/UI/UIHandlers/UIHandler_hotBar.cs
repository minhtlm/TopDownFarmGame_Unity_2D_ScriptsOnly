using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIHandler_hotbar : MonoBehaviour
{
    private UIDocument uiDocument;
    private PlayerInventory playerInventory;
    private bool isVisible = true;
    private int initialSelectedSlotIndex = 0;

    public List<VisualElement> hotBarSlots = new List<VisualElement>();

    void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        if (playerInventory == null)
        {
            playerInventory = PlayerInventory.Instance;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }
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

        SetSelectedSlot(initialSelectedSlotIndex);

        if (playerInventory == null)
        {
            playerInventory = PlayerInventory.Instance;
        }
        playerInventory.OnInventoryChanged += UpdateHotBarUI;

        UpdateHotBarUI();
    }

    void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= UpdateHotBarUI;
        }
    }

    void UpdateHotBarUI()
    {
        if (!isVisible) return;

        ClearAllSlots();

        if (playerInventory == null)
        {
            playerInventory = PlayerInventory.Instance;
        }
        List<ItemStack> playerItems = playerInventory.GetItems();

        for (int i = 0; i < hotBarSlots.Count && i < playerItems.Count; i++)
        {
            if (playerItems[i] == null) continue;

            VisualElement slot = hotBarSlots[i];
            ItemStack item = playerItems[i];

            VisualElement icon = slot.Q<VisualElement>("Item");
            if (icon != null)
            {
                icon.style.backgroundImage = item.itemDefinition.ItemSprite.texture;
            }

            Label label = slot.Q<Label>("Quantity");
            if (label != null && item.quantity > 1)
            {
                label.text = item.quantity.ToString();
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

    public void Hide()
    {
        isVisible = false;
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
        if (uiDocument != null)
        {
            uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            UpdateHotBarUI();
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
}
