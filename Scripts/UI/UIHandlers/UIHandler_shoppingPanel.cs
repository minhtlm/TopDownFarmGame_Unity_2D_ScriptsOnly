using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIHandler_shoppingPanel : IClosableUI
{
    private bool isCreated = false;
    private UIDocument uiDocument;
    private ScrollView itemsScrollView;
    private ScrollView selectedItemsScrollView;
    private VisualElement totalPanel;
    private Label totalLabel;
    private Dictionary<string, TextField> selectedItemsTextField = new Dictionary<string, TextField>();
    private List<ItemStack> selectedItems = new List<ItemStack>();
    [SerializeField] private List<ItemStack> items = new List<ItemStack>();
    [SerializeField] private UIHandler_hotbar hotbarUIHandler;

    // Start is called before the first frame update
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        itemsScrollView = uiDocument.rootVisualElement.Q<ScrollView>("ItemsScrollView");
        selectedItemsScrollView = uiDocument.rootVisualElement.Q<ScrollView>("SelectedItemsScrollView");
        totalPanel = uiDocument.rootVisualElement.Q<VisualElement>("TotalPanel");

        // Hide the panel at the start
        HideShoppingPanel();
    }

    void CreateShopInventory(ItemStack item)
    {
        VisualElement itemContainer = new VisualElement();
        itemContainer.AddToClassList("item-container");

        VisualElement dataContainer = new VisualElement();
        dataContainer.AddToClassList("data-container");

        VisualElement itemIcon = new VisualElement();
        itemIcon.style.backgroundImage = new StyleBackground(item.itemDefinition.ItemSprite.texture);
        itemIcon.AddToClassList("item-icon");

        Label itemName = new Label(item.itemDefinition.ItemName);
        itemName.AddToClassList("item-name");

        Label itemPrice = new Label(item.itemDefinition.ItemPrice.ToString() + "$");
        itemPrice.AddToClassList("item-price");

        Button buyButton = new Button();
        buyButton.text = "Buy";
        buyButton.AddToClassList("buy-button");
        buyButton.clicked += () => AddToSelectedItems(item);

        dataContainer.Add(itemIcon);
        dataContainer.Add(itemName);
        dataContainer.Add(itemPrice);
        itemContainer.Add(dataContainer);
        itemContainer.Add(buyButton);

        itemsScrollView.Add(itemContainer);
    }

    void CreateSelectedItemElement(ItemStack selectedItem)
    {
        string itemId = selectedItem.itemDefinition.itemID.ToString();

        VisualElement selectedItemContainer = new VisualElement();
        selectedItemContainer.AddToClassList("selected-item-container");

        VisualElement selectedIcon = new VisualElement();
        selectedIcon.style.backgroundImage = new StyleBackground(selectedItem.itemDefinition.ItemSprite.texture);
        selectedIcon.AddToClassList("selected-icon");

        VisualElement selectedDataContainer = new VisualElement();

        Label selectedItemName = new Label(selectedItem.itemDefinition.ItemName);
        selectedItemName.AddToClassList("selected-item-name");

        Label selectedItemPrice = new Label(selectedItem.itemDefinition.ItemPrice.ToString() + "$");
        selectedItemPrice.AddToClassList("selected-item-price");

        VisualElement selectedItemQuantityContainer = new VisualElement();
        selectedItemQuantityContainer.AddToClassList("selected-item-quantity-container");

        TextField textField = new TextField();
        textField.value = selectedItem.quantity.ToString();
        textField.AddToClassList("text-field");
        textField.name = "QuantityField";
        textField.RegisterCallback<KeyDownEvent>((evt) =>
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                ProcessQuanityChange(selectedItem, textField);
                textField.Blur();
                evt.StopPropagation();
            }
        });

        textField.RegisterCallback<FocusOutEvent>((evt) =>
        {
            ProcessQuanityChange(selectedItem, textField);
            evt.StopPropagation();
        });

        Button minusButton = new Button();
        minusButton.text = "-";
        minusButton.clicked += () => DecreaseSelectedItemQuantity(selectedItem, textField, selectedItemContainer);

        Button plusButton = new Button();
        plusButton.text = "+";
        plusButton.clicked += () => IncreaseSelectedItemQuantity(selectedItem, textField);

        selectedItemQuantityContainer.Add(minusButton);
        selectedItemQuantityContainer.Add(textField);
        selectedItemQuantityContainer.Add(plusButton);
        selectedDataContainer.Add(selectedItemName);
        selectedDataContainer.Add(selectedItemPrice);
        selectedDataContainer.Add(selectedItemQuantityContainer);
        selectedItemContainer.Add(selectedIcon);
        selectedItemContainer.Add(selectedDataContainer);

        selectedItemsScrollView.Add(selectedItemContainer);

        selectedItemsTextField.Add(itemId, textField);      
    }

    void AddToSelectedItems(ItemStack item)
    {
        ItemStack selectedItem = selectedItems.Find(i => i.itemDefinition == item.itemDefinition);
        if (selectedItem == null)
        {
            selectedItems.Add(new ItemStack(item.itemDefinition, 1));
            CreateSelectedItemElement(selectedItems[selectedItems.Count - 1]);
        }
        else
        {
            selectedItem.quantity++;

            if (selectedItemsTextField.TryGetValue(selectedItem.itemDefinition.itemID, out TextField textField))
            {
                textField.value = selectedItem.quantity.ToString();
            }
        }
        UpdateTotalPanel();
    }

    void DecreaseSelectedItemQuantity(ItemStack selectedItem, TextField textField, VisualElement selectedItemContainer, int amount = 1)
    {
        if (selectedItem.quantity > amount)
        {
            selectedItem.quantity -= amount;
            textField.value = selectedItem.quantity.ToString();
        }
        else
        {
            selectedItems.Remove(selectedItem);
            selectedItemsScrollView.Remove(selectedItemContainer);
            selectedItemsTextField.Remove(selectedItem.itemDefinition.itemID);
        }
        UpdateTotalPanel();
    }

    void IncreaseSelectedItemQuantity(ItemStack selectedItem, TextField textField, int amount = 1)
    {
        selectedItem.quantity += amount;
        textField.value = selectedItem.quantity.ToString();
        UpdateTotalPanel();
    }

    void ProcessQuanityChange(ItemStack selectedItem, TextField textField)
    {
        if (int.TryParse(textField.value, out int newQuantity) && newQuantity >= 0)
        {
            selectedItem.quantity = newQuantity;

            if (newQuantity == 0)
            {
                selectedItems.Remove(selectedItem);
                selectedItemsScrollView.Remove(textField.parent.parent.parent);
                selectedItemsTextField.Remove(selectedItem.itemDefinition.itemID);
            }
        }
        else
        {
            textField.value = selectedItem.quantity.ToString();
        }
        UpdateTotalPanel();
    }

    void CreateTotalPanel()
    {
        totalPanel.Clear();
        totalLabel = new Label();
        totalLabel.AddToClassList("total-label");
        Button payButton = new Button();
        payButton.text = "Pay";
        payButton.AddToClassList("pay-button");
        payButton.clicked += ConfirmPurchase;
        totalPanel.Add(totalLabel);
        totalPanel.Add(payButton);
    }

    void UpdateTotalPanel()
    {
        if (selectedItems.Count > 0)
        {
            totalPanel.style.display = DisplayStyle.Flex;
            int total = 0;
            foreach (ItemStack item in selectedItems)
            {
                total += item.itemDefinition.ItemPrice * item.quantity;
            }
            totalLabel.text = "Total: $" + total.ToString();
        }
        else
        {
            totalPanel.style.display = DisplayStyle.None;
        }
    }

    void ResetSelectedPanel()
    {
        selectedItems.Clear();
        selectedItemsScrollView.Clear();
        selectedItemsTextField.Clear();
        UpdateTotalPanel();
    }

    void ConfirmPurchase()
    {
        if (selectedItems.Count > 0)
        {
            PlayerStats playerStats = PlayerStats.Instance;
            if (playerStats != null)
            {
                int total = 0;
                foreach (ItemStack item in selectedItems)
                {
                    total += item.itemDefinition.ItemPrice * item.quantity;
                }

                if (playerStats.CanAfford(total))
                {
                    PlayerInventory playerInventory = PlayerInventory.Instance;
                    if (playerInventory != null)
                    {
                        foreach (ItemStack item in selectedItems)
                        {
                            playerInventory.AddItem(item.itemDefinition, item.quantity);
                        }
                        playerStats.SpendMoney(total);
                        CloseUI();
                    }
                    else
                    {
                        Debug.LogError("PlayerInventory instance not found.");
                    }
                }
                else
                {
                    Debug.Log("Not enough money to make the purchase.");
                }
            }
            else
            {
                Debug.LogError("PlayerStats instance not found.");
            }
        }
    }

    public override void ShowUI()
    {
        if (!isCreated)
        {
            foreach (ItemStack item in items)
            {
                CreateShopInventory(item);
            }
            isCreated = true;
            CreateTotalPanel();
            UpdateTotalPanel();
        }
        else
        {
            ResetSelectedPanel();
        }

        uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        IClosableUI.openingUI = this;
        PlayerController.Instance.DisableGameplayActions();

        if (hotbarUIHandler != null)
        {
            hotbarUIHandler.Hide();
        }
    }

    void HideShoppingPanel()
    {
        uiDocument.rootVisualElement.style.display = DisplayStyle.None;
        IClosableUI.openingUI = null;
    }

    public override void CloseUI()
    {
        if (IClosableUI.openingUI != null)
        {
            HideShoppingPanel();
            PlayerController.Instance.EnableGameplayActions();
            if (hotbarUIHandler != null)
            {
                hotbarUIHandler.Show();
            }
        }
    }
}
