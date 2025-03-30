using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeController : MonoBehaviour, IDestructible
{
    private ParticleSystem woodParticles;
    private bool isCut = false;
    private int health = 3;
    private string toolType = "axe";
    [SerializeField] private ItemDefinition woodDefinition;
    [SerializeField] private int woodDropAmount = 1;
    
    public GameObject leavesHitEffect;
    public Sprite stumpSprite;

    void Start()
    {
        woodParticles = GetComponentInChildren<ParticleSystem>();
    }

    public bool CanInteract(string toolType)
    {
        return !isCut && toolType == this.toolType;
    }

    public void Interact(string toolType)
    {
        if (!CanInteract(toolType)) return;
        
        HitTree();
    }

    public void OnInteractAnimation()
    {
        ShakeTree();
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void HitTree()
    {
        if (isCut) return;

        health--;
        Debug.Log("Tree health: " + health);
    }

    public void ShakeTree()
    {
        if (leavesHitEffect != null) {
            leavesHitEffect.GetComponent<Animator>().SetTrigger("LeavesHitTrigger");
        }
        if (health <= 0) {
            CutTree();
        }
    }

    public void CutTree()
    {
        if (isCut) return;
        isCut = true;

        if (woodDefinition != null)
        {
            DropWood();
        }

        GetComponent<SpriteRenderer>().sprite = stumpSprite;
        SpawnWoodParticles();
        Invoke("DestroyTree", 2.0f);
    }

    public void SpawnWoodParticles()
    {
        if (woodParticles != null) {
            woodParticles.transform.parent = null;
            woodParticles.Play();
            Destroy(woodParticles.gameObject, woodParticles.main.duration);
        }
    }

    private void DropWood()
    {
        PlayerInventory playerInventory = PlayerInventory.Instance;
        if (playerInventory != null)
        {
            playerInventory.AddItem(woodDefinition, woodDropAmount);
        }
    }

    private void DestroyTree()
    {
        Destroy(gameObject);
    }
}
