using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Data/Item")]
public class ItemDefinition : ScriptableObject
{
    private string id;
    public string itemID => id;
    public string itemName;
    public Sprite itemSprite;
    public int itemPrice = 0;
    public int maxStackSize = 1000;
    public string targetLayer = "";

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
