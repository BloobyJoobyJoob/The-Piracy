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
    public Action<float, Vector3, float, float> TriggerCannonRecoil;

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

    IEnumerator Fire(){
        shooting = true;
        yield return new WaitForSeconds(CannonSettings.CannonFireWarmUp);
        foreach (Cannon cannon in Cannons)
        {
            GameObject cannonBall = Instantiate(CannonSettings.Cannonball, cannon.cannonBallSpawn.position, cannon.cannonBallSpawn.rotation);

            cannon.particleSystem.Play();

            Vector3 angleVector = new Vector3(transform.forward.x, CannonSettings.CannonBallFireAngle, transform.forward.z).normalized;

            cannonBall.GetComponent<Rigidbody>().AddForce(angleVector * CannonSettings.CannonBallForce);

            float x = cannon.cannonBallSpawn.position.x + UnityEngine.Random.Range(-CannonSettings.CannonRecoilRandomness.x, CannonSettings.CannonRecoilRandomness.x);
            float y = cannon.cannonBallSpawn.position.y + UnityEngine.Random.Range(-CannonSettings.CannonRecoilRandomness.y, CannonSettings.CannonRecoilRandomness.y);
            float z = cannon.cannonBallSpawn.position.z;

            TriggerCannonRecoil(CannonSettings.CannonRecoilForce, new Vector3(x, y, z), CannonSettings.CannonRecoilRadius, CannonSettings.CannonRecoilUpwardsMultiplier);

            yield return new WaitForSeconds(CannonSettings.CannonFireDelay);
        }
        shooting = false;
        yield return null;
    }
}
