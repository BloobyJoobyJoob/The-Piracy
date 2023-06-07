using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShipInfo", menuName = "The Piracy/ShipInfo", order = 0)]
public class ShipInfo : ScriptableObject {
    public ShipController Ship;
    public float Mass;
    public float Drag;
    public float AngularDrag;
    public float Force;
    public float Torque;
    public float MaxTorque;
    public Vector3 CenterOfMass;
    public float BaseHealth;
    public float BuoyancyStrength;
    public float WaterHeight;
    public CameraInfo CameraInfo;
}