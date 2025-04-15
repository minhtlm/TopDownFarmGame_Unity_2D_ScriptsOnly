using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable Item", menuName = "Data/Consumable Item")]
public class ConsumableDefinition : ItemDefinition
{
    [SerializeField] private int restoreAmount = 0;
    public int RestoreAmount => restoreAmount;

    public override string GetAdditionalInfo()
    {
        return $"Restores {restoreAmount} {ItemCategory}.";
    }

    public override bool UseItem()
    {
        if (ItemCategory == ItemType.Food)
        {
            PlayerStats.Instance.AddHunger(restoreAmount);
            return true;
        }
        else if (ItemCategory == ItemType.Drink)
        {
            PlayerStats.Instance.AddThirst(restoreAmount);
            return true;
        }
        else
        {
            Debug.LogWarning("Consumable item type not recognized.");
            return false;
        }
    }
}
