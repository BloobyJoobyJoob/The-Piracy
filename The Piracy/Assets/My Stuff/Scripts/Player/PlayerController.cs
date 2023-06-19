using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;
using System;

public class PlayerController : NetworkBehaviour
{
    public ShipInfo ShipInfo { get; private set; }
    Rigidbody rb;
    CinemachineVirtualCamera virtualCamera;
    CinemachineFramingTransposer virtualCameraFollow;
    PlayerInput playerInput;
    Vector2 movement = Vector2.zero;
    bool isFiring = false;
    float scroll;

    [Tooltip("First Item is the default")]
    public ShipInfo[] Ships;
    public ShipController ShipController { get; private set; }

    public bool Spawned { get; private set; } = false;

    private NetworkVariable<int> shipIndex = new NetworkVariable<int>(value: -1, readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Owner);
    private int localShipIndex = -1;
    private void Awake() {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        
        virtualCamera = GameObject.FindGameObjectWithTag("Virtual Camera").GetComponent<CinemachineVirtualCamera>();
        virtualCameraFollow = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
    }
    public override void OnNetworkSpawn(){
        GameManager.Singleton.PlayerControllers[OwnerClientId] = this;

        if (IsOwner)
        {
            Transform center = GameObject.FindGameObjectWithTag("Map Center").transform;
            center.parent = transform;
            center.transform.localPosition = Vector3.zero;

            virtualCamera.Follow = transform;
            virtualCamera.LookAt = transform;
        }
        else
        {
            Destroy(rb);
            Destroy(playerInput);
        }
    }
    private void Start() {

        shipIndex.OnValueChanged += ShipInfoChanged;

        if (IsOwner)
        {
            MapGenerater.Singleton.loadedAllChunks += Spawn;
        }
        else
        {
            ShipInfoChanged(-1, shipIndex.Value);
        }
    }

    // Run on when the local player respawns 
    private void Spawn()
    {
        int spawnIndex = UnityEngine.Random.Range(0, MapGenerater.Singleton.SpawnLocations.Count);

        transform.position = new Vector3(MapGenerater.Singleton.SpawnLocations[spawnIndex].x, 0, MapGenerater.Singleton.SpawnLocations[spawnIndex].y);

        UpdateShipInfo(0);

        transform.position = new Vector3(transform.position.x, ShipInfo.WaterHeight, transform.position.z);

        rb.isKinematic = false;
        playerInput.enabled = true;

        Spawned = true;
    }
    // Public method for updating the ship
    public void UpdateShipInfo(int index){
        shipIndex.Value = index;
    }

    // Run on every client over the network for this ship
    private void ShipInfoChanged(int previous, int current){
        if (current != localShipIndex)
        {
            localShipIndex = current;
            if (IsOwner)
            {
                ShipInfo = Ships[current];
                rb.mass = Ships[current].Mass;
                rb.drag = Ships[current].Drag;
                rb.angularDrag = Ships[current].AngularDrag;
                rb.centerOfMass = Ships[current].CenterOfMass;

                scroll = Ships[current].CameraInfo.MaxViewDistance;
                virtualCameraFollow.m_CameraDistance = scroll;

                virtualCamera.m_Lens.FieldOfView = Ships[current].CameraInfo.FOV;
                LittlePlanetController.Singleton.UpdateCurve(Ships[current].CameraInfo.WorldCurveOffset, Ships[current].CameraInfo.WorldCurvePower);
            }

            if (ShipController != null)
            {
                Destroy(ShipController.gameObject);
            }
            ShipController = Instantiate(Ships[current].Ship.gameObject, transform).GetComponent<ShipController>();
        }
    }
    private void FixedUpdate() {

        if (IsOwner && IsSpawned && Spawned)
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
            scroll = Mathf.Clamp(scroll + (float)context.ReadValue<float>() * ShipInfo.CameraInfo.scrollSensitivity, 
                ShipInfo.CameraInfo.MinViewDistance, 
                ShipInfo.CameraInfo.MaxViewDistance);
            virtualCameraFollow.m_CameraDistance = scroll;
        }
    }
}