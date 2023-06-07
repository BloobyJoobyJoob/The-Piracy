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
    public GameObject Ship {get; private set;}

    private void Awake() {
        shipInfoChanged += OnShipInfoChanged;
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
        UpdateShipInfo(Ships[0]);

        transform.position = new Vector3(transform.position.x, ShipInfo.WaterHeight, transform.position.z);
    }
    private void FixedUpdate() {

        if (IsOwner && IsSpawned)
        {
            rb.AddForce(Vector3.forward * ShipInfo.Force * Mathf.Clamp(movement.y, 0, 1) * Time.timeScale);

            Vector3 torque = Vector3.up * ShipInfo.Torque * movement.x * Time.timeScale * rb.velocity.x;

            rb.AddTorque(new Vector3(0, Mathf.Clamp(torque.y, -ShipInfo.MaxTorque, ShipInfo.MaxTorque), 0));

            if (transform.position.y < ShipInfo.WaterHeight)
            {
                rb.AddForce(Vector3.up * ShipInfo.BuoyancyStrength * (ShipInfo.WaterHeight - transform.position.y), ForceMode.VelocityChange);
            }
        }
    }
    public void UpdateShipInfo(ShipInfo info){
        shipInfoChanged.Invoke(info);
    }

    public void UpdateShipInfo(int ShipInfoIndex){
        UpdateShipInfo(Ships[ShipInfoIndex]);
    }

    private void OnShipInfoChanged(ShipInfo info){
        ShipInfo = info;
        rb.mass = info.Mass;
        rb.drag = info.Drag;
        rb.angularDrag = info.AngularDrag;
        rb.centerOfMass = info.CenterOfMass;
        
        scroll = info.CameraInfo.MaxViewDistance;
        virtualCameraFollow.m_CameraDistance = scroll;
        
        virtualCamera.m_Lens.FieldOfView = info.CameraInfo.FOV;

        Destroy(Ship);
        Ship = Instantiate(info.Ship.gameObject, transform);
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
