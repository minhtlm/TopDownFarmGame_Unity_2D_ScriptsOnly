using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopHouseController : MonoBehaviour
{
    [SerializeField] private Vector2 insidePosition;
    [SerializeField] private Vector2 outsidePosition;
    private bool playerInTriggerArea = false;
    private bool playerInside = false;
    

    public static ShopHouseController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInTriggerArea = true;
            Debug.Log("Player in trigger area");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInTriggerArea = false;
        }
    }

    public void EnterShopHouse(GameObject player)
    {
        if (playerInTriggerArea && player != null)
        {
            if (!playerInside)
            {
                playerInside = true;
                player.transform.position = insidePosition;
                Debug.Log("Player entered shop house");
            }
        }
    }
}