using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterTransform : MonoBehaviour
{
    public Transform center;
    public float recenterDelay = 1;

    void Start(){
        InvokeRepeating("CenterOceanFunc", 1, recenterDelay);
    }

    void CenterOceanFunc(){
        transform.position = new Vector3(Mathf.RoundToInt(center.position.x), 
            transform.position.y,
            Mathf.RoundToInt(center.position.z));

    }
}
