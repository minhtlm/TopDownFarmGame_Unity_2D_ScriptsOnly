using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemStack
{
    public ItemDefinition itemDefinition;
    public int quantity;

    public ItemStack(ItemDefinition item, int amount = 1)
    {
        itemDefinition = item;
        quantity = amount;
    }
}

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private List<ItemStack> items = new List<ItemStack>();
    private int maxInventorySlots = 36;

    // Event to trigger when the inventory changes
    public event Action OnInventoryChanged;

    public static PlayerInventory Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    public List<ItemStack> GetItems()
    {
        return items;
    }

    public bool AddItem(ItemDefinition itemToAdd, int amount)
    {
        if (itemToAdd == null) return false;

        // Check if the item is already in the inventory
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemDefinition == itemToAdd)
            {
                items[i].quantity += amount;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        if (items.Count < maxInventorySlots)
        {
            items.Add(new ItemStack(itemToAdd, amount));
            OnInventoryChanged?.Invoke();
            return true;
        }

        Debug.Log("Inventory is full!");
        return false;
    }

    public bool RemoveItem(ItemDefinition itemToRemove, int amount)
    {
        if (itemToRemove == null) return false;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemDefinition == itemToRemove)
            {
                items[i].quantity -= amount;
                if (items[i].quantity <= 0)
                {
                    items.RemoveAt(i);
                }

                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        return false;
    }

    public bool HasItem(ItemDefinition itemToCheck, int minAmount)
    {
        if (itemToCheck == null) return false;

        foreach (ItemStack item in items)
        {
            if (item.itemDefinition == itemToCheck && item.quantity >= minAmount)
            {
                return true;
            }
        }

        return false;
    }

    public void SwapItems(int sourceIndex, int targetIndex)
    {
        Debug.Log("Swapping started");
        List<ItemStack> playerItems = GetItems();

        // If the target index is within the inventory
        if (targetIndex < playerItems.Count)
        {
            ItemStack draggedItem = playerItems[sourceIndex];
            ItemStack targetItem = playerItems[targetIndex];

            if (draggedItem != null && targetItem != null && draggedItem.itemDefinition == targetItem.itemDefinition)
            {
                int totalQuantity = draggedItem.quantity + targetItem.quantity;
                int maxStack = draggedItem.itemDefinition.maxStackSize;

                if (totalQuantity <= maxStack)
                {
                    targetItem.quantity = totalQuantity;
                    playerItems[sourceIndex] = null;
                }
                else
                {
                    targetItem.quantity = maxStack;
                    draggedItem.quantity = totalQuantity - maxStack;
                }
            } else
            {
                playerItems[sourceIndex] = targetItem;
                playerItems[targetIndex] = draggedItem;
            }

        }
        else
        {
            ItemStack draggedItem = playerItems[sourceIndex];

            // Fill in the empty slots and add the item
            while (playerItems.Count < targetIndex)
            {
                playerItems.Add(null);
            }
            playerItems[sourceIndex] = null;
            playerItems.Add(draggedItem);
        }

        OnInventoryChanged?.Invoke();
        Debug.Log("Swapping finished");
    }
}
