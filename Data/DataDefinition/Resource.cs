using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Resource", menuName = "Data/Resource")]
public class Resource : ItemDefinition
{
    public override string GetAdditionalInfo()
    {
        return "";
    }

    public override bool UseItem()
    {
        return true;
    }
}
