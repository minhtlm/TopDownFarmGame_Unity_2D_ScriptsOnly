using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class DoorController : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject transitionZone;
    [SerializeField] private GameObject nextMap;

    public void OnInteract()
    {
        // UIHandler_screenFader uiScreenFader = UIHandler_screenFader.Instance;
        // StartCoroutine(uiScreenFader.FadeTransition(null, () => {
        //     PlayerController.Instance.transform.position = teleportPosition;
        // }));

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
