using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ShipController : MonoBehaviour
{
    public Transform[] floatPoints;
    public ShipUpgrades SpeedUpgrades;
    public ShipUpgrades CannonDamageUpgrades;
    public ShipUpgrades CannonSpeedUpgrades;
    public ShipUpgrades CannonRangeUpgrades;
    public ShipUpgrades CannonQuantityUpgrades;
    public ShipUpgrades ShipArmor;
    public ShipUpgrades ShipHealth;
    public ShipUpgrades GoodsQuantityUpgrades;
    public ShipUpgrades GoodsStorageUpgrades;
    public Cannon[] Cannons;
    public CannonSettings CannonSettings;
    public bool shooting {get; private set;}
    public Action<Vector3> TriggerCannonRecoil;

    public Action ShipHitAction;

    public bool FireCannons(){
        if (shooting)
        {
            return false;
        }
        else
        {
            StartCoroutine(Fire());
            return true;
        }
    }

    public void ShipHit(){
        ShipHitAction();
    }

    IEnumerator Fire(){
        shooting = true;
        yield return new WaitForSeconds(CannonSettings.CannonFireWarmUp);
        foreach (Cannon cannon in Cannons)
        {
            GameObject cannonBallObject = Instantiate(CannonSettings.Cannonball, cannon.cannonBallSpawn.position, cannon.cannonBallSpawn.rotation);

            Cannonball cannonball = cannonBallObject.GetComponent<Cannonball>();

            cannon.particleSystem.Play();

            Vector3 angleVector = new Vector3(transform.forward.x, CannonSettings.CannonBallFireAngle, transform.forward.z).normalized;

            cannonball.Constructor(this, angleVector * CannonSettings.CannonBallForce);

            TriggerCannonRecoil(transform.forward * -CannonSettings.CannonRecoilForce);

            yield return new WaitForSeconds(CannonSettings.CannonFireDelay);
        }
        shooting = false;
        yield return null;
    }
}
