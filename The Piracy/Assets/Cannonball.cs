using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannonball : MonoBehaviour
{
    public ParticleSystem ParticleSystem;
    public Rigidbody Rigidbody;
    public float WaterHeight;
    public float UnderwaterVelocityChange = 0.98f;

    public float ParticalEmmissionRateMultiplier;

    private void Update() {
        ParticleSystem.EmissionModule emissionModule = ParticleSystem.emission;
        emissionModule.rateOverTime = ParticalEmmissionRateMultiplier * Rigidbody.velocity.magnitude;

        if (transform.position.y < WaterHeight)
        {
            emissionModule.rateOverTime = 0;
        }
    }
    private void FixedUpdate() {
        if (transform.position.y < WaterHeight)
        {
            Rigidbody.velocity = Rigidbody.velocity * UnderwaterVelocityChange;
        }
    }
}
