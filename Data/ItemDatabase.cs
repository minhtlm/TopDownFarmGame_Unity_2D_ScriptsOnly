using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance { get; private set; }

    private Dictionary<string, ItemDefinition> itemDictionary = new Dictionary<string, ItemDefinition>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllItems();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadAllItems()
    {
        ItemDefinition[] allItems = Resources.LoadAll<ItemDefinition>("Datas");
        foreach (ItemDefinition item in allItems)
        {
            if (!itemDictionary.ContainsKey(item.itemID))
            {
                itemDictionary.Add(item.itemID, item);
            }
        }
    }

    public ItemDefinition GetItemById(string itemID)
    {
        return itemDictionary.TryGetValue(itemID, out ItemDefinition item) ? item : null;
    }
}
