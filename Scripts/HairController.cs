using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HairController : MonoBehaviour
{
    private Dictionary<string, Sprite> hairDictionary;

    public SpriteRenderer hairSpriteRenderer;
    public List<Sprite> hairStylesList;

    void Awake()
    {
        hairDictionary = new Dictionary<string, Sprite>();
        foreach (Sprite hairStyle in hairStylesList)
        {
            if (!hairDictionary.ContainsKey(hairStyle.name))
                hairDictionary.Add(hairStyle.name, hairStyle);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeHairStyle(string hairStyleName)
    {
        if (hairDictionary.ContainsKey(hairStyleName))
        {
            hairSpriteRenderer.sprite = hairDictionary[hairStyleName];
        } else
        {
            Debug.Log("Hair style not found: " + hairStyleName);
        }
    }
}
