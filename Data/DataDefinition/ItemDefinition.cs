using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemDefinition : ScriptableObject
{
    [SerializeField] private string id;
    public string itemID => id;

    [SerializeField] private string itemName;
    public string ItemName => itemName;

    [SerializeField] private Sprite itemSprite;
    public Sprite ItemSprite => itemSprite;

    [SerializeField] private int itemPrice = 0;
    public int ItemPrice => itemPrice;

    [SerializeField] private int maxStackSize = 100;
    public int MaxStackSize => maxStackSize;

    [SerializeField] private string description = "";
    public string Description => description;

    [SerializeField] private ItemType itemType = ItemType.Miscellaneous;
    public ItemType ItemCategory => itemType;

    public abstract string GetAdditionalInfo();
    public abstract bool UseItem();

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this); // Mark as dirty so it gets saved to asset
#endif
        }
    }
}

public enum ItemType
{
    Miscellaneous,
    Tool,
    Food,
    Drink
}
