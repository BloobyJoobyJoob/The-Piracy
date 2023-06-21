using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    public Transform[] floatPoints;
    public ShipUpgrades SpeedUpgrades;
    public ShipUpgrades CannonDamageUpgrades;
    public ShipUpgrades CannonSpeedUpgrades;
    public ShipUpgrades CannonRangeUpgrades;
    public ShipUpgrades CannonQuantityUpgrades;
    public ShipUpgrades HullIntergrityUpgrades;
    public ShipUpgrades GoodsQuantityUpgrades;
    public ShipUpgrades GoodsStorageUpgrades;

    public Cannon[] Cannons;

    public CannonSettings CannonSettings;

    public bool shooting {get; private set;}

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
            GameObject cannonBall = Instantiate(CannonSettings.Cannonball, cannon.cannonBallSpawn.transform.position, cannon.cannonBallSpawn.transform.rotation);

            cannon.particleSystem.Play();

            Vector3 angleVector = new Vector3(transform.forward.x, CannonSettings.CannonBallFireAngle, transform.forward.z).normalized;

            cannonBall.GetComponent<Rigidbody>().AddForce(angleVector * CannonSettings.CannonBallForce);

            yield return new WaitForSeconds(CannonSettings.CannonFireDelay);
        }
        shooting = false;
        yield return null;
    }
}
