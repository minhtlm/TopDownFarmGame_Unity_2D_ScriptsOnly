using System.Collections;
using System.Collections.Generic;
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

    public void SpawnWoodParticles()
    {
        if (woodParticles != null) {
            woodParticles.transform.parent = null;
            woodParticles.Play();
            Destroy(woodParticles.gameObject, woodParticles.main.duration);
        }
    } 
}
