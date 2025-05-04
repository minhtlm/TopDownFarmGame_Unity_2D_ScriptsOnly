using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTransition : MonoBehaviour
{
    [SerializeField] private GameObject transitionZone;
    [SerializeField] private GameObject nextMap;


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(UIHandler_screenFader.Instance.FadeTransition(
                () => {
                    PlayerController.Instance.transform.position = transitionZone.transform.position; // Move the player to the new map's transition zone
                },
                () => {
                    transform.root.gameObject.SetActive(false); // Deactivate the current map
                    nextMap.SetActive(true); // Activate the next map
                }
            ));
        }
    }
}

