using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShipCollider : MonoBehaviour
{
    public ShipController shipController;

    public void ShipHit(){
        shipController.ShipHit();
    }

    private void OnCollisionEnter(Collision other) {
        if (NetworkManager.Singleton.IsServer)
        {
            Cannonball cannonball;
            if (TryGetComponent<Cannonball>(out cannonball))
            {
                if (cannonball.Owner != shipController)
                {
                    cannonball.Owner.ShipHit();
                }
            }
        }
    }
}
