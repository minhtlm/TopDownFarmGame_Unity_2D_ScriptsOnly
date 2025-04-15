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

        SetSelectedSlot(initialSelectedSlotIndex);

        playerInventory.OnInventoryChanged += UpdateHotBarUI;

        UpdateHotBarUI();
    }

    void OnDestroy()
    {
        playerInventory.OnInventoryChanged -= UpdateHotBarUI;
    }

    void UpdateHotBarUI()
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
