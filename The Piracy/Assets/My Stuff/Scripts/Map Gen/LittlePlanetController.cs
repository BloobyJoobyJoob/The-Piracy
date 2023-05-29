using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LittlePlanetController : MonoBehaviour
{
    public float CurvePower;
    public string CurvePowerRef;
    public float CurveOffset;
    public string CurveOffsetRef;
    public Transform center;
    public string CurveCenterRef;

    public static LittlePlanetController Singleton;

    private int CurvePowerID;
    private int CurveOffsetID;
    private int CurveCenterID;

    void Awake()
    {
        Singleton = this;

        CurvePowerID = Shader.PropertyToID(CurvePowerRef);
        CurveOffsetID = Shader.PropertyToID(CurveOffsetRef);
        CurveCenterID = Shader.PropertyToID(CurveCenterRef);

        UpdateCurve(CurveOffset, CurvePower);
    }

    public void UpdateCurve(float offset, float power)
    {
        Shader.SetGlobalFloat(CurveOffsetID, offset);
        Shader.SetGlobalFloat(CurvePowerID, power);
    }

    void Update()
    {
        Shader.SetGlobalVector(CurveCenterID, new Vector2(center.position.x, center.position.z));
    }
}
