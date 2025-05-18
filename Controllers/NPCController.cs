using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, IInteractable
{
    private GameObject expression;
    [SerializeReference] private IClosableUI ClosableUI;

    // Start is called before the first frame update
    void Start()
    {
        expression = transform.Find("Expression").gameObject;
        if (expression != null)
        {
            expression.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (expression != null)
            {
                expression.SetActive(true);
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

    public void OnInteract()
    {
        ClosableUI.ShowUI();
    }
}
