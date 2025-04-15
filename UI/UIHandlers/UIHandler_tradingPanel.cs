using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIHandler_tradingPanel : IClosableUI
{
    private int totalPrice;
    private UIDocument uiDocument;
    private VisualElement totalPanel;
    private Label totalLabel;
    private ScrollView itemsScrollView;
    private ScrollView selectedItemsScrollView;
    private List<ItemStack> selectedItems = new List<ItemStack>();
    private List<ItemStack> playerItems = new List<ItemStack>();
    private Dictionary<string, VisualElement> playerItemElements = new Dictionary<string, VisualElement>();
    private Dictionary<string, VisualElement> selectedItemElements = new Dictionary<string, VisualElement>();
    [SerializeField] private UIHandler_hotbar hotBarManager;

    // Start is called before the first frame update
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        itemsScrollView = uiDocument.rootVisualElement.Q<ScrollView>("ItemsScrollView");
        selectedItemsScrollView = uiDocument.rootVisualElement.Q<ScrollView>("SelectedItemsScrollView");
        totalPanel = uiDocument.rootVisualElement.Q<VisualElement>("TotalPanel");

        HideTradingPanel();
    }

    void CreatePlayerItemElement(ItemStack item)
    {
        string itemID = item.itemDefinition.itemID;

        VisualElement itemContainer = new VisualElement();
        itemContainer.AddToClassList("item-container");

        VisualElement dataContainer = new VisualElement();
        dataContainer.AddToClassList("data-container");

        VisualElement icon = new VisualElement();
        icon.style.backgroundImage = new StyleBackground(item.itemDefinition.ItemSprite.texture);
        icon.AddToClassList("icon-sprite");

        Label itemName = new Label(item.itemDefinition.ItemName);
        itemName.AddToClassList("item-name");

        Label itemPrice = new Label(item.itemDefinition.ItemPrice.ToString() + "$");
        itemPrice.AddToClassList("item-price");

        Label itemQuantity = new Label("Quantity: " + item.quantity.ToString());
        itemQuantity.AddToClassList("item-quantity");
        itemQuantity.name = "item-quantity";

        Button sellButton = new Button();
        sellButton.text = "Sell";
        sellButton.AddToClassList("sell-button");
        sellButton.clicked += () => TransferItem(item, playerItems, selectedItems, 1);

        dataContainer.Add(icon);
        dataContainer.Add(itemName);
        dataContainer.Add(itemPrice);
        dataContainer.Add(itemQuantity);
        itemContainer.Add(dataContainer);
        itemContainer.Add(sellButton);

        itemsScrollView.Add(itemContainer);

        playerItemElements[itemID] = itemContainer;
    }

    void CreateSelectedItemElement(ItemStack item)
    {
        string itemID = item.itemDefinition.itemID;

        VisualElement selectedItemContainer = new VisualElement();
        selectedItemContainer.AddToClassList("selected-item-container");

        VisualElement selectedIcon = new VisualElement();
        selectedIcon.AddToClassList("selected-icon");
        selectedIcon.style.backgroundImage = new StyleBackground(item.itemDefinition.ItemSprite.texture);

        VisualElement selectedDataContainer = new VisualElement();

        Label selectedItemName = new Label(item.itemDefinition.ItemName);
        selectedItemName.AddToClassList("selected-item-name");

        Label selectedItemPrice = new Label(item.itemDefinition.ItemPrice.ToString() + "$");
        selectedItemPrice.AddToClassList("selected-item-price");

        VisualElement selectedItemQuantityContainer = new VisualElement();
        selectedItemQuantityContainer.AddToClassList("selected-item-quantity-container");

        Button minusButton = new Button();
        minusButton.text = "-";
        minusButton.clicked += () => TransferItem(item, selectedItems, playerItems, 1);

        Button plusButton = new Button();
        plusButton.text = "+";
        plusButton.clicked += () => PlusQuantity(item);

        TextField textField = new TextField();
        textField.value = item.quantity.ToString();
        textField.AddToClassList("text-field");
        textField.name = "quantity-field";

        textField.RegisterCallback<KeyDownEvent>((evt) =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                ProcessQuanityChange(item, textField);
                textField.Blur();
                evt.StopPropagation();
            }
        });

        textField.RegisterCallback<FocusOutEvent>((evt) =>
        {
            ProcessQuanityChange(item, textField);
            evt.StopPropagation();
        });

        selectedItemQuantityContainer.Add(minusButton);
        selectedItemQuantityContainer.Add(textField);
        selectedItemQuantityContainer.Add(plusButton);
        selectedDataContainer.Add(selectedItemName);
        selectedDataContainer.Add(selectedItemPrice);
        selectedDataContainer.Add(selectedItemQuantityContainer);
        selectedItemContainer.Add(selectedIcon);
        selectedItemContainer.Add(selectedDataContainer);

        selectedItemsScrollView.Add(selectedItemContainer);

        selectedItemElements[itemID] = selectedItemContainer;
    }

    void ProcessQuanityChange(ItemStack item, TextField textField)
    {
        if (int.TryParse(textField.value, out int newQuantity) && newQuantity >= 0)
        {
            int currentQuantity = item.quantity;
            int quantityDifference = newQuantity - currentQuantity;

            if (quantityDifference == 0) return; // No change in quantity

            ItemStack playerItem = playerItems.Find(i => i.itemDefinition == item.itemDefinition);

            if (quantityDifference > 0) // Increase the quantity
            {
                if (playerItem == null) // No item in the player's inventory
                {
                    textField.value = item.quantity.ToString();
                    return;
                }

                if (playerItem.quantity >= quantityDifference)
                {
                    // Transfer the item from the player's inventory to the selected items
                    TransferItem(playerItem, playerItems, selectedItems, quantityDifference);
                }
                else
                {
                    // Transfer the maximum possible quantity
                    TransferItem(playerItem, playerItems, selectedItems, playerItem.quantity);
                }
            }
            else if (quantityDifference < 0) // Decrease the quantity
            {
                // Transfer the item from the selected items to the player's inventory
                TransferItem(item, selectedItems, playerItems, -quantityDifference);
            }
        }
        else
        {
            // Reset the quantity to the previous value
            textField.value = item.quantity.ToString();
        }
    }

    void UpdatePlayerItemUI(ItemStack item)
    {
        // Check if the item is already in the player's inventory
        if (playerItemElements.ContainsKey(item.itemDefinition.itemID))
        {
            string itemId = item.itemDefinition.itemID;

            if (item.quantity <= 0)
            {
                // Remove the item from the player's inventory if its quantity is 0
                if (playerItemElements.TryGetValue(itemId, out VisualElement element))
                {
                    playerItemElements.Remove(itemId);
                    itemsScrollView.Remove(element);
                }
                playerItems.Remove(item);

                if (playerItemElements.Count <= 0)
                {
                    CreateEmptyInventoryLabel();
                }
            }
            else if (playerItemElements.TryGetValue(itemId, out VisualElement playerItemElement))
            {
                // Update the quantity of the item in the player's inventory
                Label quantityLabel = playerItemElement.Q<Label>("item-quantity");
                if (quantityLabel != null)
                {
                    quantityLabel.text = "Quantity: " + item.quantity.ToString();
                }
            }
        }
        else
        {
            // Create a new element for the item in the player's inventory
            CreatePlayerItemElement(item);
        }
    }

    void UpdateSelectedItemUI(ItemStack item)
    {
        if (selectedItemElements.ContainsKey(item.itemDefinition.itemID))
        {
            string itemId = item.itemDefinition.itemID;

            if (item.quantity <= 0)
            {
                // Remove the item from the selected items if its quantity is 0
                if (selectedItemElements.TryGetValue(itemId, out VisualElement element))
                {
                    selectedItemElements.Remove(itemId);
                    selectedItemsScrollView.Remove(element);
                }
            }
            else if (selectedItemElements.TryGetValue(itemId, out VisualElement selectedItemElement))
            {
                // Update the quantity of the item in the selected items
                TextField quantityField = selectedItemElement.Q<TextField>("quantity-field");
                if (quantityField != null)
                {
                    quantityField.value = item.quantity.ToString();
                }
            }
        }
        else
        {
            CreateSelectedItemElement(item);
        }
    }

    void PlusQuantity(ItemStack item)
    {
        ItemStack playerItem = playerItems.Find(i => i.itemDefinition == item.itemDefinition);
        if (playerItem == null || playerItem.quantity <= 0) return;

        TransferItem(playerItem, playerItems, selectedItems, 1);
    }

    void TransferItem(ItemStack sourceItem, List<ItemStack> sourceList, List<ItemStack> targetList, int quantity)
    {
        if (sourceItem == null || sourceItem.quantity < quantity || quantity <= 0) return;

        // Decrease the quantity of the source item
        sourceItem.quantity -= quantity;

        // Increase the quantity of the target item or add a new item to the target list
        ItemStack targetItem = targetList.Find(i => i.itemDefinition == sourceItem.itemDefinition);
        if (targetItem != null)
        {
            targetItem.quantity += quantity;
        }
        else
        {
            targetItem = new ItemStack(sourceItem.itemDefinition, quantity);
            targetList.Add(targetItem);
        }
        
        // Update the total price and UI elements
        if (sourceList == playerItems && targetList == selectedItems)
        {
            totalPrice += sourceItem.itemDefinition.ItemPrice * quantity;
            UpdatePlayerItemUI(sourceItem);
            UpdateSelectedItemUI(targetItem);
        }
        else if (sourceList == selectedItems && targetList == playerItems)
        {
            totalPrice -= sourceItem.itemDefinition.ItemPrice * quantity;
            UpdateSelectedItemUI(sourceItem);
            UpdatePlayerItemUI(targetItem);
        }

        // Remove the item from the source list if its quantity is 0
        if (sourceItem.quantity <= 0)
        {
            sourceList.Remove(sourceItem);
        }
        UpdateTotalPanel();
    }

    void CreateTotalPanel()
    {
        totalPanel.Clear();
        totalLabel = new Label("Total : $" + totalPrice.ToString());
        totalLabel.AddToClassList("total-label");
        Button totalSellButton = new Button();
        totalSellButton.text = "Sell All";
        totalSellButton.AddToClassList("total-sell-button");
        totalSellButton.clicked += () => ConfirmTransaction();
        totalPanel.Add(totalLabel);
        totalPanel.Add(totalSellButton);
    }

    void UpdateTotalPanel()
    {
        if (selectedItems.Count > 0)
        {
            totalPanel.style.display = DisplayStyle.Flex;
            if (totalLabel != null)
            {
                totalLabel.text = "Total : $" + totalPrice.ToString();
            }
        }
        else
        {
            totalPanel.style.display = DisplayStyle.None;
        }
    }

    void ConfirmTransaction()
    {
        if (selectedItems.Count <= 0 || totalPrice <= 0)
        {
            Debug.Log("No items to sell!");
            return;
        }

        PlayerStats playerStats = PlayerStats.Instance;
        if (playerStats != null)
        {
            playerStats.AddMoney(totalPrice);
            foreach (ItemStack item in selectedItems)
            {
                PlayerInventory.Instance.RemoveItem(item.itemDefinition, item.quantity);
            }
            CloseUI();
        }   
    }

    void CreateEmptyInventoryLabel()
    {
        Label emptyInventoryLabel = new Label("Nothing to sell!");
        emptyInventoryLabel.AddToClassList("empty-inventory-label");
        itemsScrollView.Clear();
        itemsScrollView.Add(emptyInventoryLabel);
    }

    void RebuildTradingPanel()
    {
        CreateTotalPanel();
        playerItemElements.Clear();
        itemsScrollView.Clear();
        selectedItemElements.Clear();
        selectedItemsScrollView.Clear();
        selectedItems.Clear();
        playerItems.Clear();
        totalPrice = 0;
        UpdateTotalPanel();

        // Create the copy of the player's inventory items
        PlayerInventory playerInventory = PlayerInventory.Instance;
        if (playerInventory != null)
        {
            List<ItemStack> playerInventoryItems = new List<ItemStack>(playerInventory.GetItems());
            foreach (ItemStack item in playerInventoryItems)
            {
                if (item == null || item.quantity <= 0) continue;
                playerItems.Add(new ItemStack(item.itemDefinition, item.quantity));
            }
        }

        if (playerItems.Count <= 0)
        {
            CreateEmptyInventoryLabel();
            return;
        }

        // Recreate the player items UI elements
        foreach (ItemStack item in playerItems)
        {
            if (item == null || item.quantity <= 0) continue;
            CreatePlayerItemElement(item);
        }
    }

    public override void ShowUI()
    {
        RebuildTradingPanel();
        uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        IClosableUI.openingUI = this;
        PlayerController.Instance.DisableGameplayActions();

        if (hotBarManager != null)
        {
            hotBarManager.Hide();
        }
    }

    public void HideTradingPanel()
    {
        uiDocument.rootVisualElement.style.display = DisplayStyle.None;
        IClosableUI.openingUI = null;

        if (hotBarManager != null)
        {
            hotBarManager.Show();
        }
    }

    public override void CloseUI()
    {
        if (IClosableUI.openingUI != null)
        {
            HideTradingPanel();
            PlayerController.Instance.EnableGameplayActions();
        }
    }
}
