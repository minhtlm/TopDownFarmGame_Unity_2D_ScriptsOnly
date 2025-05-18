using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class VirtualCameraConfiner : MonoBehaviour
{
    private CinemachineConfiner2D confiner2D;

    public static VirtualCameraConfiner Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        confiner2D = GetComponent<CinemachineConfiner2D>();
    }

    public void SetConfiner2D(Vector3 position)
    {
        Collider2D collider = Physics2D.OverlapPoint(position, LayerMask.GetMask("Confiner")); // Check if the position is within a confiner collider
        if (collider != null)
        {
            confiner2D.m_BoundingShape2D = collider; // Set the confiner to the collider found
            confiner2D.InvalidateCache(); // Invalidate the cache to ensure the new confiner is used
        }
        else
        {
            Debug.LogWarning("No confiner found at the specified position.");
        }
    }
}
