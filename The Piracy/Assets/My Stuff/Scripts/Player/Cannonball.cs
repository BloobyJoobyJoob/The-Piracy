using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Cannonball : MonoBehaviour
{
    public ParticleSystem ParticleSystem;
    public Rigidbody Rigidbody;
    public float WaterHeight;
    public float UnderwaterVelocityChange = 0.98f;
    public float DestroyDelay = 3;
    public float ParticalEmmissionRateMultiplier;

    bool underWater = false;
    private void Update() {
        ParticleSystem.EmissionModule emissionModule = ParticleSystem.emission;
        if (transform.position.y < WaterHeight)
        {
            if (!underWater)
            {
                underWater = true;
                emissionModule.rateOverTime = 0;
                Destroy(gameObject, DestroyDelay);
            }
        }
        else
        {
            emissionModule.rateOverTime = ParticalEmmissionRateMultiplier * Rigidbody.velocity.magnitude;
        }
    }
    private void FixedUpdate() {
        if (transform.position.y < WaterHeight)
        {
            Rigidbody.velocity = Rigidbody.velocity * UnderwaterVelocityChange;
        }
    }
}
