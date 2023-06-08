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
    float worldSize;
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
        worldSize = float.Parse(data["WorldSize"].Value);
        
        MapGenerater.Singleton.SetMapInformation(seed, octaves, persistance, scale, lacunarity);

        Cursor.lockState = CursorLockMode.Locked;
        
        if (NetworkManager.Singleton.IsServer)
        {
            SpawnPlayer(NetworkManager.ServerClientId);
        }
    }

    //Runs Server side
    private void SpawnPlayer(ulong id){

        Vector3 spawnPosition = new Vector3(Random.Range(-worldSize, worldSize), 0, Random.Range(-worldSize, worldSize));

        Debug.Log(spawnPosition);
        NetworkObject player = Instantiate(PlayerPrefab, spawnPosition, Quaternion.identity).GetComponent<NetworkObject>();
        player.SpawnWithOwnership(id);
    }

    void OnClientConnected(ulong id){
        
        if (NetworkManager.Singleton.IsServer)
        {
            SpawnPlayer(id);
        }
    }
}
