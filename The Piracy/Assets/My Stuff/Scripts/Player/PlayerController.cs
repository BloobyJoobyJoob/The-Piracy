using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;
using System;

public class PlayerController : NetworkBehaviour
{
    public Action<ShipInfo> shipInfoChanged;
    public ShipInfo ShipInfo {get; private set;}
    Rigidbody rb;
    CinemachineVirtualCamera virtualCamera;
    CinemachineFramingTransposer virtualCameraFollow;
    PlayerInput playerInput;
    Vector2 movement = Vector2.zero;
    bool isFiring = false;
    float scroll;

    [Tooltip("First Item is the default")]
    public ShipInfo[] Ships;
    public ShipController ShipController {get; private set;}

    private void Awake() {

    }
    public override void OnNetworkSpawn(){
        GameManager.Singleton.PlayerControllers[OwnerClientId] = this;

        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

        if (IsOwner)
        {
            Transform center = GameObject.FindGameObjectWithTag("Map Center").transform;
            center.parent = transform;
            center.transform.localPosition = Vector3.zero;

            virtualCamera = GameObject.FindGameObjectWithTag("Virtual Camera").GetComponent<CinemachineVirtualCamera>();
            virtualCamera.Follow = transform;
            virtualCamera.LookAt = transform;

            virtualCameraFollow = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
        else
        {
            Destroy(rb);
            Destroy(playerInput);
        }
    }
    private void Start() {
        UpdateShipInfo(Ships[0], true);

        transform.position = new Vector3(transform.position.x, ShipInfo.WaterHeight, transform.position.z);
        Debug.Log(transform.position);
    }
    private void FixedUpdate() {

        if (IsOwner && IsSpawned)
        {
            rb.AddForce(new Vector3(transform.forward.x, 0, transform.forward.z) * ShipInfo.Force * Mathf.Clamp(movement.y, 0, 1) * Time.timeScale);

            Vector3 torque = Vector3.up * ShipInfo.Torque * movement.x * Time.timeScale * new Vector2(rb.velocity.x, rb.velocity.z).magnitude;

            rb.AddTorque(new Vector3(0, Mathf.Clamp(torque.y, -ShipInfo.MaxTorque, ShipInfo.MaxTorque), 0));

            FloatShip(ShipController.floatPoints);
        }
    }

    void FloatShip(Transform[] floatingPoints){
        
        foreach (Transform point in floatingPoints)
        {
            if (point.position.y < ShipInfo.WaterHeight)
            {
                rb.AddForceAtPosition(Vector3.up * ShipInfo.BuoyancyStrength * (ShipInfo.WaterHeight - point.position.y), point.position, ForceMode.VelocityChange);
            }
        }
    } 
    public void UpdateShipInfo(ShipInfo info, bool firstSpawn = false){
        ShipInfo = info;
        rb.mass = info.Mass;
        rb.drag = info.Drag;
        rb.angularDrag = info.AngularDrag;
        rb.centerOfMass = info.CenterOfMass;
        
        scroll = info.CameraInfo.MaxViewDistance;
        virtualCameraFollow.m_CameraDistance = scroll;
        
        virtualCamera.m_Lens.FieldOfView = info.CameraInfo.FOV;
        
        if (!firstSpawn)
        {
            Destroy(ShipController.gameObject);  
        }

        ShipController = Instantiate(info.Ship.gameObject, transform).GetComponent<ShipController>();

        try
        {
           shipInfoChanged.Invoke(ShipInfo); 
        }
        catch {}
    }

    public void UpdateShipInfo(int ShipInfoIndex, bool firstSpawn = false){
        UpdateShipInfo(Ships[ShipInfoIndex], firstSpawn);
    }

    public void OnMove(InputAction.CallbackContext context){
        if (IsOwner && IsSpawned)
        {
            movement = context.ReadValue<Vector2>();
        }
    }
    public void OnFire(InputAction.CallbackContext context){
        if (IsOwner && IsSpawned)
        {
           isFiring = context.performed;
        }
    }

    public void OnScroll(InputAction.CallbackContext context){
        if (IsOwner && IsSpawned)
        {
            scroll = Mathf.Clamp(scroll + (float)context.ReadValue<int>() * ShipInfo.CameraInfo.scrollSensitivity, 
                ShipInfo.CameraInfo.MinViewDistance, 
                ShipInfo.CameraInfo.MaxViewDistance);
            virtualCameraFollow.m_CameraDistance = scroll;
        }
    }
}
