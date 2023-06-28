using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Cannonball : MonoBehaviour
{
    public ParticleSystem ParticleSystem;
    public MeshRenderer MeshRenderer;
    public Rigidbody Rigidbody;
    public float WaterHeight;
    public float UnderwaterVelocityChange = 0.98f;
    public float DestroyDelay = 3;
    public float BoundsScale = 10;
    public float ParticalEmmissionRateMultiplier;
    public ShipController Owner;
    bool underWater = false;

    private void Start()
    {
        Bounds b = MeshRenderer.bounds;
        b.size *= BoundsScale;
    }

    public void Constructor(ShipController owner, Vector3 force){
        Owner = owner;
        Rigidbody.AddForce(force);
    }

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
