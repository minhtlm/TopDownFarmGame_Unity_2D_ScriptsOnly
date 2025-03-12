using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TreeController : MonoBehaviour
{
    private ParticleSystem woodParticles;
    
    public bool isCut = false;
    public int health = 3;
    public GameObject leavesHitEffect;
    public Sprite stumpSprite;

    void Start()
    {
        woodParticles = GetComponentInChildren<ParticleSystem>();
    }

    public void HitTree()
    {
        if (isCut) return;

        health--;
    }

    public void ShakeTree()
    {
        transform.DOShakePosition(0.5f, 0.2f);
        if (leavesHitEffect != null) {
            leavesHitEffect.GetComponent<Animator>().SetTrigger("LeavesHitTrigger");
        }
        if (health <= 0) {
            CutTree();
        }
    }

    public void CutTree()
    {
        isCut = true;
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

    private void DestroyTree()
    {
        Destroy(gameObject);
    }
}
