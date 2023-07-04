using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShipCollider : MonoBehaviour
{
    public ShipController shipController;

    private void OnCollisionEnter(Collision other) {
        if (NetworkManager.Singleton.IsServer)
        {
            if (other.gameObject.TryGetComponent<Cannonball>(out Cannonball cannonball))
            {
                if (cannonball.Owner != shipController)
                {
                    shipController.ShipHit();
                }
            }
        }
    }
}
