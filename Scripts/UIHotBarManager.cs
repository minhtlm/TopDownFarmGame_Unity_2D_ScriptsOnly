using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIHotBarManager : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;
    private List<VisualElement> hotBarSlots = new List<VisualElement>();
    private PlayerInventory playerInventory;
    private bool isVisible = true;

    // Start is called before the first frame update
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

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

        if (playerInventory == null)
        {
            playerInventory = PlayerInventory.Instance;
            if (playerInventory == null)
            {
                Debug.LogError("PlayerInventory not found");
                return;
            }
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

        List<ItemStack> playerItems = playerInventory.GetItems();
        Debug.Log("Player items: " + playerItems.Count);

        for (int i = 0; i < hotBarSlots.Count && i < playerItems.Count; i++)
        {
            if (playerItems[i] == null) continue;

            VisualElement slot = hotBarSlots[i];
            ItemStack item = playerItems[i];

            VisualElement icon = slot.Q<VisualElement>("Item");
            if (icon != null)
            {
                icon.style.backgroundImage = item.itemDefinition.itemSprite.texture;
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
        root.style.display = DisplayStyle.None;
    }

    public void Show()
    {
        isVisible = true;
        root.style.display = DisplayStyle.Flex;
        UpdateHotBarUI();
    }
}
