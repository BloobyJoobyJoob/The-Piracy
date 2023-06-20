using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraSettings", menuName = "The Piracy/CameraSettings", order = 0)]
public class CameraSettings : ScriptableObject
{
    public float FOV;
    public Vector3 FollowOffset;
    public float MaxViewDistance;
    public float MinViewDistance;
    public float scrollSensitivity;

    public float WorldCurvePower;
    public float WorldCurveOffset;
}