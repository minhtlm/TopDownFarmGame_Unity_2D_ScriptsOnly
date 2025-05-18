using System.Collections;
using System.Collections.Generic;
using Cinemachine;
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
                preFadeAction: () => {
                    nextMap.SetActive(true); // Activate the next map
                },
                teleportAction: () => {
                    PlayerController.Instance.transform.position = transitionZone.transform.position; // Move the player to the new map's transition zone
                    VirtualCameraConfiner.Instance.SetConfiner2D(PlayerController.Instance.transform.position); // Set the confiner to the player's position
                },
                afterFadeAction: () => {
                    transform.root.gameObject.SetActive(false); // Deactivate the current map
                }
            ));
        }
    }
}

