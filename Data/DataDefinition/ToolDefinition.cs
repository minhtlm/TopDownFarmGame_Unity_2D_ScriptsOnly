using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool", menuName = "Data/Tool")]
public class ToolDefinition : ItemDefinition
{
    [SerializeField] private string targetLayer = "";
    public string TargetLayer => targetLayer;

    [SerializeField] private string animatorTrigger = "";
    public string AnimatorTrigger => animatorTrigger;

    public override string GetAdditionalInfo()
    {
        return "";
    }

    public override bool UseItem()
    {
        return true;
    }
}
