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
        if (IsOwner)
        {
            MapGenerater.Singleton.loadedAllChunks += Spawn;
        }
    }

    // Run on when the local player respawns 
    private void Spawn()
    {
        int spawnIndex = UnityEngine.Random.Range(0, MapGenerater.Singleton.SpawnLocations.Count);

        transform.position = new Vector3(MapGenerater.Singleton.SpawnLocations[spawnIndex].x, 0, MapGenerater.Singleton.SpawnLocations[spawnIndex].y);

        UpdateShipInfoServerRPC(0);
        UpdateShipInfo(0);

        transform.position = new Vector3(transform.position.x, ShipInfo.WaterHeight, transform.position.z);

        rb.isKinematic = false;
        playerInput.enabled = true;

        Spawned = true;
    }
    [ServerRpc]
    private void UpdateShipInfoServerRPC(int index, ServerRpcParams serverRpcParams = default){
        UpdateShipInfoClientRPC(index, serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void UpdateShipInfoClientRPC(int index, ulong sender)
    {
        Debug.Log(sender);
        Debug.Log(OwnerClientId);
        if (NetworkManager.LocalClientId != sender) UpdateShipInfo(index);
    }
    void UpdateShipInfo(int index){

        ShipInfo = Ships[index];
        rb.mass = Ships[index].Mass;
        rb.drag = Ships[index].Drag;
        rb.angularDrag = Ships[index].AngularDrag;
        rb.centerOfMass = Ships[index].CenterOfMass;

        scroll = Ships[index].CameraInfo.MaxViewDistance;
        virtualCameraFollow.m_CameraDistance = scroll;

        virtualCamera.m_Lens.FieldOfView = Ships[index].CameraInfo.FOV;
        LittlePlanetController.Singleton.UpdateCurve(Ships[index].CameraInfo.WorldCurveOffset, Ships[index].CameraInfo.WorldCurvePower);

        if (ShipController != null)
        {
            Destroy(ShipController.gameObject);
        }

        Debug.Log("Spawning Ship! " + OwnerClientId);
        ShipController = Instantiate(Ships[index].Ship.gameObject, transform).GetComponent<ShipController>();

        try
        {
            shipInfoChanged.Invoke(ShipInfo);
        }
        catch { }
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
    public void UpgradeShip(int ShipInfoIndex){
        UpdateShipInfoServerRPC(ShipInfoIndex);
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