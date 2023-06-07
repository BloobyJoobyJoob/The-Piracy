using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraInfo", menuName = "The Piracy/CameraInfo", order = 0)]
public class CameraInfo : ScriptableObject
{
    public float FOV;
    public Vector3 FollowOffset;
    public float MaxViewDistance;
    public float MinViewDistance;
    public float scrollSensitivity;
}