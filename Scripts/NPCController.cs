using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    private GameObject expression;

    // Start is called before the first frame update
    void Start()
    {
        expression = GetComponentInChildren<SpriteRenderer>().gameObject;
        if (expression != null)
        {
            expression.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player entered NPC trigger");
            if (expression != null)
            {
                expression.SetActive(true);
                Debug.Log("Expression activated");
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (expression != null)
            {
                expression.SetActive(false);
            }
        }
    }
}
