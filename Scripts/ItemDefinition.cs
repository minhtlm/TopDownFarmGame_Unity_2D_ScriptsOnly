using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Data/Item")]
public class ItemDefinition : ScriptableObject
{
    public string itemName;
    public Sprite itemSprite;
    public int maxStackSize = 1000;
}
