using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton = null;
    public GameObject PlayerPrefab;
    public PlayerController[] PlayerControllers = new PlayerController[100];
    public float WorldSize {get; private set;}
    void Awake(){
        if (Singleton != null){
            Debug.LogError("poos");
        }
        Singleton = this;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    void Start(){
        var data = GameNetworkHelper.Singleton.clientLobby.Data;

        int seed = int.Parse(data["Seed"].Value);
        int octaves = int.Parse(data["NoiseOctaves"].Value);
        float persistance = float.Parse(data["NoisePersistence"].Value);
        float scale = float.Parse(data["NoiseScale"].Value);
        float lacunarity = float.Parse(data["NoiseLacunarity"].Value);
        WorldSize = float.Parse(data["WorldSize"].Value);

        int worldChunkWidth = Mathf.FloorToInt(WorldSize * 0.5f / MapGenerater.Singleton.size);


        int offsetX = Random.Range(-worldChunkWidth, worldChunkWidth);
        int offsetY = Random.Range(-worldChunkWidth, worldChunkWidth);

        MapGenerater.Singleton.SetMapInformation(seed, octaves, persistance, scale, lacunarity, new Vector2(
            offsetX,
            offsetY)
        );

        offsetX *= -MapGenerater.Singleton.size;
        offsetY *= -MapGenerater.Singleton.size;

        BoxCollider boxCollider;
        
        boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.center = new Vector3(WorldSize * 0.5f - offsetX, 0, -offsetY);
        boxCollider.size = new Vector3(0, WorldSize, WorldSize);

        boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.center = new Vector3(-offsetX, 0, WorldSize * 0.5f - offsetY);
        boxCollider.size = new Vector3(WorldSize, WorldSize, 0);

        boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.center = new Vector3(WorldSize * -0.5f - offsetX, 0, -offsetY);
        boxCollider.size = new Vector3(0, WorldSize, WorldSize);

        boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.center = new Vector3(-offsetX, 0, WorldSize * -0.5f - offsetY);
        boxCollider.size = new Vector3(WorldSize, WorldSize, 0);

        Cursor.lockState = CursorLockMode.Locked;
        
        if (NetworkManager.Singleton.IsServer)
        {
            SpawnPlayer(NetworkManager.ServerClientId);
        }
    }

    //Runs Server side
    private void SpawnPlayer(ulong id){

        Debug.Log("Player Spawned!");
        NetworkObject player = Instantiate(PlayerPrefab).GetComponent<NetworkObject>();
        player.SpawnWithOwnership(id);
    }
    void OnClientConnected(ulong id){
        
        if (NetworkManager.Singleton.IsServer)
        {
            SpawnPlayer(id);
        } 
    }
}
