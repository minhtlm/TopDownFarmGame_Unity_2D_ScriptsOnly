using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodParticlesFix : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject ground = GameObject.FindWithTag("Ground");
        if (ground != null)
        {
            ParticleSystem particle = GetComponent<ParticleSystem>();
            if (particle != null)
            {
                var collision = particle.collision;
                collision.enabled = true;
                collision.type = ParticleSystemCollisionType.Planes;
                collision.SetPlane(0, ground.transform);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
