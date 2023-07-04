using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CannonSettings", menuName = "The Piracy/CannonSettings", order = 0)]
public class CannonSettings : ScriptableObject {
    public float CannonFireWarmUp;
    public float CannonFireDelay;
    public float CannonBallForce;
    public float CannonBallFireAngle;
    public float CannonRecoilForce = 5;
    public GameObject Cannonball; 
}