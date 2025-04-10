using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockController : MonoBehaviour, IDestructible
{
    private bool isMined = false;
    private int health = 2;
    private string toolType = "pickaxe";
    [SerializeField] private ItemDefinition stoneDefinition;
    [SerializeField] private int stoneDropAmount = 1;

    void Start()
    {
        
    }
    public bool CanInteract(string toolType)
    {
        return !isMined && toolType == this.toolType;
    }

    public void Interact(string toolType)
    {
        if (!CanInteract(toolType)) return;

        HitRock();
    }

    public void OnInteractAnimation()
    {
        if (health <= 0) {
            MineRock();
        }
    }

    public void HitRock()
    {
        if (isMined) return;

        health--;
        Debug.Log("Rock health: " + health);
    }

    public void MineRock()
    {
        if (isMined) return;
        isMined = true;

        if (stoneDefinition != null)
        {
            DropStone();
        }
        Destroy(gameObject);
    }

    void DropStone()
    {
        PlayerInventory playerInventory = PlayerInventory.Instance;
        if (playerInventory != null)
        {
            playerInventory.AddItem(stoneDefinition, stoneDropAmount);
        }
    }
}
