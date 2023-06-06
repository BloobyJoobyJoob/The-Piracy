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
    PlayerInput playerInput;
    Vector2 movement = Vector2.zero;
    bool isFiring = false;

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
            Destroy(rb);
            Destroy(playerInput);
        }
    }
    private void FixedUpdate() {
        rb.AddForce(Vector3.forward * ShipInfo.Acceloration * Mathf.Clamp(movement.x, 0, 1) * Time.timeScale);
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

        Destroy(Ship);
        Ship = Instantiate(info.Ship.gameObject);
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
}
