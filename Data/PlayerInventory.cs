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
    private List<ItemStack> items = new List<ItemStack>();
    private int maxInventorySlots = 36;
    
    public event Action OnInventoryChanged; // Event to trigger when the inventory changes

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

        DontDestroyOnLoad(gameObject);
    }

    public List<ItemStack> GetItems()
    {
        return items;
    }

    public ItemStack GetItem(int index)
    {
        if (index >= 0 && index < items.Count)
        {
            return items[index];
        }

        return null; // Return null if index is out of range
    }

    public bool AddItem(ItemDefinition itemToAdd, int amount = 1)
    {
        if (itemToAdd == null) return false;
        int firstEmptySlot = -1;

        // Check if the item is already in the inventory
        for (int i = 0; i < items.Count; i++)
        {
            if (IsEmptySlot(i)) // Find the first empty slot
            {
                if (firstEmptySlot == -1) firstEmptySlot = i;
                continue; // Skip null items
            }

            ItemStack item = items[i];
            if (item.itemDefinition == itemToAdd && item.quantity + amount < item.itemDefinition.MaxStackSize)
            {
                item.quantity += amount;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        if (firstEmptySlot != -1) // If there's an empty slot, add the item there
        {
            items[firstEmptySlot] = new ItemStack(itemToAdd, amount);
            OnInventoryChanged?.Invoke();
            return true;
        }

        if (items.Count < maxInventorySlots) // If there's no empty slot add the item to the end of the list
        {
            items.Add(new ItemStack(itemToAdd, amount));
            OnInventoryChanged?.Invoke();
            return true;
        }

        Debug.Log("Inventory is full!");
        return false;
    }

    public bool RemoveItem(ItemDefinition itemToRemove, int amount = 1)
    {
        if (itemToRemove == null) return false;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null) continue; // Skip null items

            if (items[i].itemDefinition == itemToRemove)
            {
                items[i].quantity -= amount;
                if (items[i].quantity <= 0)
                {
                    items[i] = null; // Remove the item from the inventory
                }

                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        return false;
    }

    public bool HasItem(ItemDefinition itemToCheck, int minAmount = 1)
    {
        if (itemToCheck == null) return false;

        foreach (ItemStack item in items)
        {
            if (item == null) continue; // Skip null items

            if (item.itemDefinition == itemToCheck && item.quantity >= minAmount)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsEmptySlot(int index)
    {
        if (index < 0 || index >= items.Count) return true; // Check if the index is invalid

        return items[index] == null || items[index].itemDefinition == null; // Check if the slot is empty
    }

    public void SwapItems(int sourceIndex, int targetIndex)
    {
        List<ItemStack> playerItems = GetItems();

        // If the target index is within the inventory
        if (targetIndex < playerItems.Count)
        {
            ItemStack draggedItem = playerItems[sourceIndex];
            ItemStack targetItem = playerItems[targetIndex];

            if (draggedItem != null && targetItem != null && draggedItem.itemDefinition == targetItem.itemDefinition)
            {
                int totalQuantity = draggedItem.quantity + targetItem.quantity;
                int maxStack = draggedItem.itemDefinition.MaxStackSize;

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
    }

    public List<ItemStackData> ToSerializableData()
    {
        List<ItemStackData> itemStackDataList = new List<ItemStackData>();

        foreach (ItemStack itemStack in items)
        {
            if (itemStack != null && itemStack.itemDefinition != null)
            {
                ItemStackData itemStackData = new ItemStackData
                {
                    itemID = itemStack.itemDefinition.itemID,
                    quantity = itemStack.quantity
                };
                itemStackDataList.Add(itemStackData);
            }
        }

        return itemStackDataList;
    }

    public void LoadFromSerializableData(List<ItemStackData> itemStackDataList)
    {
        // Search for the item in the database and add it to the inventory
        foreach (ItemStackData itemStackData in itemStackDataList)
        {
            ItemDefinition itemDefinition = ItemDatabase.Instance.GetItemById(itemStackData.itemID);
            if (itemDefinition != null)
            {
                ItemStack itemStack = new ItemStack(itemDefinition, itemStackData.quantity);
                items.Add(itemStack);
            }
        }

        OnInventoryChanged?.Invoke();
    }
}
