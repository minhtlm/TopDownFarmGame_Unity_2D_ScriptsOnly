using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolController : MonoBehaviour
{
    private Dictionary<string, Sprite> toolDictionary;

    public SpriteRenderer toolSpriteRenderer;
    public List<Sprite> toolList;

    void Awake()
    {
        toolDictionary = new Dictionary<string, Sprite>();
        foreach (Sprite tool in toolList)
        {
            if (!toolDictionary.ContainsKey(tool.name))
                toolDictionary.Add(tool.name, tool);
        }
    }

    public void ChangeTool(string toolName)
    {
        if (toolDictionary.ContainsKey(toolName))
        {
            toolSpriteRenderer.sprite = toolDictionary[toolName];
        } else
        {
            Debug.Log("Tool not found: " + toolName);
        }
    }
}
