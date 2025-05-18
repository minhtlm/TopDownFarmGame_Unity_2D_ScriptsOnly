using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weighted Item", menuName = "Data/Weighted Item")]
public class WeightedItem : ItemDefinition
{
    [SerializeField] private int weight = 0;
    public int Weight => weight;
    
    public override string GetAdditionalInfo()
    {
        throw new System.NotImplementedException();
    }

    public override bool UseItem()
    {
        throw new System.NotImplementedException();
    }
}
