using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class DoorController : MonoBehaviour, IInteractable
{
    [SerializeField] private Vector2 teleportPosition;

    public void OnInteract()
    {
        UIHandler_screenFader uiScreenFader = UIHandler_screenFader.Instance;
        StartCoroutine(uiScreenFader.FadeTransition(() => {
            PlayerController.Instance.transform.position = teleportPosition;
        }));
    }
}
