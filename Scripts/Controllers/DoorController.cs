using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class DoorController : MonoBehaviour, IInteractable
{
    [SerializeField] private Vector2 teleportPosition;
    private GameObject player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void OnInteract()
    {
        if (player != null) 
        {
            UIHandler_screenFader uiScreenFader = UIHandler_screenFader.Instance;
            StartCoroutine(uiScreenFader.FadeTransition(() => {
                player.transform.position = teleportPosition;
            }));
        }
    }
}
